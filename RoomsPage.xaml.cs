using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GestionHotel
{
    public partial class RoomsPage : Page
    {
        private string connectionString = "Server=localhost;Database=HotelLib;Uid=root;Pwd=;";
        private Rectangle selectedRectangle; // Suivre le rectangle sélectionné
        private int? selectedRoomId; // ID de la chambre sélectionnée

        public RoomsPage()
        {
            InitializeComponent();
            LoadRoomsData();
        }

        // Charger les chambres depuis la base de données et afficher des rectangles
        private void LoadRoomsData()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Chambres"; // Récupérer toutes les chambres
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    // Vider les anciens rectangles
                    RoomsWrapPanel.Children.Clear();
                    selectedRectangle = null;
                    selectedRoomId = null;

                    while (reader.Read())
                    {
                        int roomId = Convert.ToInt32(reader["chambreID"]);
                        string roomNumber = reader["numchambre"].ToString();
                        string status = reader["statut"].ToString(); // 'Occupee' ou 'Libre'
                        string roomType = reader["typechambre"].ToString();

                        // Créer un rectangle pour chaque chambre avec la couleur appropriée
                        Brush roomColor = status.ToLower() == "occupee" || status.ToLower() == "occupée"
                        ? new SolidColorBrush(Color.FromRgb(255, 182, 193))
                        : Brushes.White;

                        Rectangle roomRectangle = new Rectangle
                        {
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(10),
                            Fill = roomColor,
                            Stroke = Brushes.Black,
                            StrokeThickness = 1,
                            Tag = new { RoomId = roomId, Status = status } // Tag pour stocker l'ID et le statut de la chambre
                        };

                        // Ajouter un événement de clic pour sélectionner la chambre
                        roomRectangle.MouseLeftButtonUp += RoomRectangle_Click;

                        // Ajouter un texte pour afficher le numéro et le type de la chambre
                        TextBlock roomNumberText = new TextBlock
                        {
                            Text = $"{roomNumber}\n{roomType}",
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = Brushes.Black,
                            TextAlignment = TextAlignment.Center
                        };

                        // Conteneur pour le rectangle et le texte
                        Grid roomContainer = new Grid
                        {
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(10)
                        };
                        roomContainer.Children.Add(roomRectangle);
                        roomContainer.Children.Add(roomNumberText);

                        RoomsWrapPanel.Children.Add(roomContainer);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des chambres : {ex.Message}");
                }
            }
        }

        // Gestion du clic sur un rectangle
        private void RoomRectangle_Click(object sender, MouseButtonEventArgs e)
        {
            if (selectedRectangle != null)
            {
                // Réinitialiser la bordure de l'ancien rectangle sélectionné
                selectedRectangle.StrokeThickness = 1;

                // Restaurer la couleur d'origine basée sur le statut
                var previousTag = (dynamic)selectedRectangle.Tag;
                string previousStatus = previousTag.Status;

                selectedRectangle.Fill = previousStatus.ToLower() == "occupee" || previousStatus.ToLower() == "occupée"
                ? new SolidColorBrush(Color.FromRgb(255, 182, 193))
                : Brushes.White;
            }

            // Mettre à jour le rectangle sélectionné
            selectedRectangle = sender as Rectangle;
            selectedRectangle.StrokeThickness = 3; // Indiquer la sélection avec une bordure épaisse
            selectedRectangle.Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)); // Bleu clair pour la sélection
            var currentTag = (dynamic)selectedRectangle.Tag;
            selectedRoomId = currentTag.RoomId; // Récupérer l'ID de la chambre sélectionnée
        }

        // Fonction pour ajouter une chambre
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // NavigationService.Navigate(new AddOrModifyRoomPage(null));
            var addPage = new AddOrModifyRoomPage(null); // Créez la page d'ajout
            addPage.RoomDataChanged += LoadRoomsData;  // S'abonner à l'événement pour recharger les chambres après l'ajout
            NavigationService.Navigate(addPage);
        }

        // Fonction pour modifier une chambre
        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoomId.HasValue)
            {
                var page = new AddOrModifyRoomPage(selectedRoomId.Value);
                page.RoomDataChanged += LoadRoomsData; // Souscrire à l'événement pour recharger les chambres
                NavigationService.Navigate(page); // Naviguer avec l'ID de la chambre sélectionnée
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une chambre à modifier.");
            }
        }

        // Fonction pour supprimer une chambre
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoomId.HasValue)
            {
                var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cette chambre ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();
                            string query = "DELETE FROM Chambres WHERE chambreID = @id";
                            MySqlCommand command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@id", selectedRoomId.Value);
                            command.ExecuteNonQuery();

                            MessageBox.Show("Chambre supprimée avec succès !");
                            LoadRoomsData(); // Recharger les données
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erreur lors de la suppression de la chambre : {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une chambre à supprimer.");
            }
        }       

    }
}
