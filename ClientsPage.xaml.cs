using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GestionHotel
{
    public partial class ClientsPage : Page
    {
        private readonly string connectionString = "Server=localhost;Database=HotelLib;Uid=root;Pwd=;";
        private bool isLoading = false;

        public ClientsPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadClientsDataAsync();
        }

        // Méthode asynchrone pour charger les données
        private async Task LoadClientsDataAsync(string filter = null)
        {
            if (isLoading) return;
            isLoading = true;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                string query = @"SELECT 
                                CAST(clientsID AS SIGNED) as Id, 
                                Prenom, 
                                Nom, 
                                Email, 
                                Telephone, 
                                Adresse, 
                                CAST(date_naissance AS DATETIME) as DateNaissance 
                               FROM Clients";

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query += " WHERE Nom LIKE @filter OR Prenom LIKE @filter OR Email LIKE @filter";
                }

                using var command = new MySqlCommand(query, connection);
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    command.Parameters.AddWithValue("@filter", $"%{filter}%");
                }

                using var adapter = new MySqlDataAdapter(command);
                var dataTable = new DataTable();
                await Task.Run(() => adapter.Fill(dataTable));

                ClientsDataGrid.ItemsSource = dataTable.DefaultView;
            }
            catch (MySqlException ex)
            {
                ShowError("Erreur de base de données", $"Une erreur est survenue lors de l'accès à la base de données : {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowError("Erreur inattendue", $"Une erreur inattendue est survenue : {ex.Message}");
            }
            finally
            {
                Mouse.OverrideCursor = null;
                isLoading = false;
            }
        }

        // Méthode asynchrone pour supprimer un client
        private async Task DeleteClientAsync(int clientId)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                string query = "DELETE FROM Clients WHERE clientsID = @id";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", clientId);

                await command.ExecuteNonQueryAsync();

                ShowSuccess("Client supprimé avec succès");
                await LoadClientsDataAsync();
            }
            catch (MySqlException ex)
            {
                ShowError("Erreur de suppression", $"Impossible de supprimer le client : {ex.Message}");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        // Gestionnaires d'événements
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddOrModifyClientPage(null, async () => await LoadClientsDataAsync()));
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is not DataRowView selectedClient)
            {
                ShowWarning("Sélection requise", "Veuillez sélectionner un client à modifier.");
                return;
            }

            int clientId = Convert.ToInt32(selectedClient["Id"]);
            NavigationService?.Navigate(new AddOrModifyClientPage(clientId, async () => await LoadClientsDataAsync()));
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is not DataRowView selectedClient)
            {
                ShowWarning("Sélection requise", "Veuillez sélectionner un client à supprimer.");
                return;
            }

            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir supprimer ce client ?",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int clientId = Convert.ToInt32(selectedClient["Id"]);
                await DeleteClientAsync(clientId);
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text;
            if (searchText.Length >= 2) // Commence la recherche à partir de 2 caractères
            {
                await LoadClientsDataAsync(searchText);
            }
            else if (string.IsNullOrEmpty(searchText))
            {
                await LoadClientsDataAsync();
            }
        }

       

        // Ajout du gestionnaire d'événements manquant pour ClientsDataGrid_SelectionChanged
        private void ClientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is DataRowView selectedClient)
            {
                // Met à jour l'état des boutons en fonction de la sélection
                // Vous pouvez personnaliser cette logique selon vos besoins
                bool isItemSelected = selectedClient != null;
                if (FindName("ModifyButton") is Button modifyButton)
                    modifyButton.IsEnabled = isItemSelected;
                if (FindName("DeleteButton") is Button deleteButton)
                    deleteButton.IsEnabled = isItemSelected;
            }
        }

        // Méthodes utilitaires pour afficher les messages
        private void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowWarning(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}