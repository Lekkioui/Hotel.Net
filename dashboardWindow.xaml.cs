using LiveCharts.Wpf;
using LiveCharts;
using System;
using MySql.Data.MySqlClient;
using System.Windows;

namespace GestionHotel
{
    public partial class dashboardWindow : Window
    {
        public string CurrentUsername { get; set; }
        public dashboardWindow(string username)
        {
            InitializeComponent();
            LoadChart();
            CurrentUsername = username;
            SetWelcomeMessage();
        }

        private void SetWelcomeMessage()
        {
            WelcomeTextBlock.Text = $"Bienvenue {CurrentUsername}";
        }

        private void LoadChart()
        {
            // Récupérer les données
            int clientsCount = GetCountFromDatabase("Clients");
            int reservationsCount = GetCountFromDatabase("Reservations");
            int roomsCount = GetCountFromDatabase("Chambres");
            int usersCount = GetCountFromDatabase("Utilisateurs");

            // Trouver la valeur maximale pour l'échelle
            int maxValue = Math.Max(Math.Max(clientsCount, reservationsCount),
                                  Math.Max(roomsCount, usersCount));

            // Définir les séries de données pour le graphe
            StatsChart.Series = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Clients",
            Values = new ChartValues<int> { clientsCount },
            MaxColumnWidth = 100, // Augmentation de la largeur des colonnes
            Fill = System.Windows.Media.Brushes.DodgerBlue,
            DataLabels = true, // Afficher les valeurs sur les colonnes
            LabelPoint = point => point.Y.ToString() // Format d'affichage des valeurs
        },
        new ColumnSeries
        {
            Title = "Réservations",
            Values = new ChartValues<int> { reservationsCount },
            MaxColumnWidth = 100,
            Fill = System.Windows.Media.Brushes.OrangeRed,
            DataLabels = true,
            LabelPoint = point => point.Y.ToString()
        },
        new ColumnSeries
        {
            Title = "Chambres",
            Values = new ChartValues<int> { roomsCount },
            MaxColumnWidth = 100,
            Fill = System.Windows.Media.Brushes.ForestGreen,
            DataLabels = true,
            LabelPoint = point => point.Y.ToString()
        },
        new ColumnSeries
        {
            Title = "Utilisateurs",
            Values = new ChartValues<int> { usersCount },
            MaxColumnWidth = 100,
            Fill = System.Windows.Media.Brushes.Purple,
            DataLabels = true,
            LabelPoint = point => point.Y.ToString()
        }
    };

            // Configurer l'axe X
            StatsChart.AxisX = new AxesCollection
    {
        new Axis
        {
            Title = "Catégories",
            Labels = new[] { "Clients", "Réservations", "Chambres", "Utilisateurs" },
            Separator = new Separator
            {
                Step = 1,
                IsEnabled = false // Désactive les lignes verticales
            }
        }
    };

            // Configurer l'axe Y avec une échelle adaptée
            StatsChart.AxisY = new AxesCollection
    {
        new Axis
        {
            Title = "Nombre",
            MinValue = 0,
            MaxValue = maxValue + (maxValue * 0.1), // Ajoute 10% d'espace en haut
            LabelFormatter = value => value.ToString("0"),
            Separator = new Separator
            {
                Step = Math.Max(1, maxValue / 10) // Divise l'échelle en 10 parties environ
            }
        }
    };

            // Configuration supplémentaire du graphique
            StatsChart.LegendLocation = LegendLocation.Right;
            StatsChart.DataTooltip = null; // Désactive les tooltips car on a déjà les labels
            StatsChart.AnimationsSpeed = TimeSpan.FromMilliseconds(500);
        }
    

        private int GetCountFromDatabase(string tableName)
        {
            string connectionString = "Server=localhost;Database=HotelLib;Uid=root;Pwd=;";

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = $"SELECT COUNT(*) FROM {tableName}";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la récupération des données : {ex.Message}");
                    return 0;
                }
            }
        }

        private void ResetDashboard()
        {
            // Vider le Frame
            MainFrame.Content = null;

            // Afficher le graphique
            StatsChart.Visibility = Visibility.Visible;

            // Recharger les données du graphique
            LoadChart();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void BtnDasboard_Click(object sender, RoutedEventArgs e)
        {
            ResetDashboard();
        }

        private void BtnClients_Click(object sender, RoutedEventArgs e)
        {
            StatsChart.Visibility = Visibility.Collapsed;
            MainFrame.Navigate(new ClientsPage());
        }

        private void BtnRooms_Click(object sender, RoutedEventArgs e)
        {
            StatsChart.Visibility = Visibility.Collapsed;
            MainFrame.Navigate(new RoomsPage());
        }

        private void BtnReservations_Click(object sender, RoutedEventArgs e)
        {
            StatsChart.Visibility = Visibility.Collapsed;
            MainFrame.Navigate(new ReservationsPage());
        }

        private void BtnUtilisateurs_Click(object sender, RoutedEventArgs e)
        {
            StatsChart.Visibility = Visibility.Collapsed;
            MainFrame.Navigate(new UtilisateursPage());
        }
        private void BtnContracts_Click(object sender, RoutedEventArgs e)
        {
            // Naviguer vers la page des contrats
            MainFrame.Navigate(new ContractsPage());
        }

    }
}