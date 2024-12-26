using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GestionHotel
{
    public partial class AddOrModifyRoomPage : Page
    {
        private string connectionString = "Server=localhost;Database=HotelLib;Uid=root;Pwd=;";
        private int? roomId;
        public event Action RoomDataChanged;  // Déclaration de l'événement

        public AddOrModifyRoomPage(int? roomId)
        {
            InitializeComponent();
            this.roomId = roomId;

            if (roomId.HasValue)
            {
                LoadRoomData(roomId.Value);
            }
        }

        // Charger les données d'une chambre existante
        private void LoadRoomData(int roomId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Chambres WHERE chambreID = @id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", roomId);
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        // Charger le numéro de chambre
                        RoomNumberTextBox.Text = reader["numchambre"].ToString();

                        // Charger le type de chambre et le sélectionner dans le ComboBox
                        string roomType = reader["typechambre"].ToString();
                        RoomTypeComboBox.SelectedItem = RoomTypeComboBox.Items.Cast<ComboBoxItem>()
                            .FirstOrDefault(item => ((ComboBoxItem)item).Content.ToString() == roomType);

                        // Charger le prix
                        PriceTextBox.Text = reader["prix"].ToString();

                        // Charger le statut de la chambre et le sélectionner dans le ComboBox
                        string status = reader["statut"].ToString();
                        RoomStatusComboBox.SelectedItem = RoomStatusComboBox.Items.Cast<ComboBoxItem>()
                            .FirstOrDefault(item => ((ComboBoxItem)item).Content.ToString() == status);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}");
                }
            }
        }

        // Enregistrer ou modifier une chambre
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string roomNumber = RoomNumberTextBox.Text;
            string roomType = ((ComboBoxItem)RoomTypeComboBox.SelectedItem)?.Content.ToString();
            string status = ((ComboBoxItem)RoomStatusComboBox.SelectedItem).Content.ToString();
            string price = PriceTextBox.Text;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query;

                    if (roomId.HasValue)
                    {
                        query = "UPDATE Chambres SET numchambre = @roomNumber, typechambre = @roomType, statut = @status, prix = @price WHERE chambreID = @id";
                    }
                    else
                    {
                        query = "INSERT INTO Chambres (numchambre, typechambre, statut, prix) VALUES (@roomNumber, @roomType, @status, @price)";
                    }

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@roomNumber", roomNumber);
                    command.Parameters.AddWithValue("@roomType", roomType);
                    command.Parameters.AddWithValue("@status", status);
                    command.Parameters.AddWithValue("@price", price);

                    if (roomId.HasValue)
                    {
                        command.Parameters.AddWithValue("@id", roomId.Value);
                    }

                    command.ExecuteNonQuery();

                    MessageBox.Show("Chambre enregistrée avec succès !");
                    RoomDataChanged?.Invoke(); // Déclencher l'événement après l'enregistrement
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'enregistrement de la chambre : {ex.Message}");
                }
            }
        }

        // Annuler et retourner à la page précédente
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
