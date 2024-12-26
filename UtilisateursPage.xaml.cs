using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace GestionHotel
{
    public partial class UtilisateursPage : Page
    {
        private string connectionString = "Server=localhost;Database=HotelLib;Uid=root;Pwd=;"; // Remplace par ta chaîne de connexion

        public UtilisateursPage()
        {
            InitializeComponent();
            LoadUsersData();
        }

        private void LoadUsersData()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Id, Username, Password FROM Utilisateurs";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    UsersDataGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des utilisateurs : {ex.Message}");
                }
            }
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as DataRowView;
            if (selectedUser != null)
            {
                // Charger les valeurs dans les TextBox et PasswordBox pour modification
                UsernameTextBox.Text = selectedUser["Username"].ToString();
                PasswordBox.Password = selectedUser["Password"].ToString();
            }
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as DataRowView;
            if (selectedUser == null)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur à modifier.");
                return;
            }

            // Validation si les TextBox sont non vides avant d'envoyer les données
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Le nom d'utilisateur et le mot de passe ne peuvent pas être vides.");
                return;
            }

            string updatedUsername = UsernameTextBox.Text;
            string updatedPassword = PasswordBox.Password;

            int userId;
            if (!int.TryParse(selectedUser["Id"].ToString(), out userId))
            {
                MessageBox.Show("Erreur lors de la conversion de l'ID utilisateur.");
                return;
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Utilisateurs SET Username = @username, Password = @password WHERE Id = @id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", userId);
                    command.Parameters.AddWithValue("@username", updatedUsername);
                    command.Parameters.AddWithValue("@password", updatedPassword);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Utilisateur modifié avec succès !");
                        // Mettre à jour le DataGrid
                        selectedUser["Username"] = updatedUsername;
                        selectedUser["Password"] = updatedPassword;
                        UsersDataGrid.Items.Refresh();

                        // Vider les TextBox et PasswordBox
                        UsernameTextBox.Text = string.Empty;
                        PasswordBox.Password = string.Empty;
                    }
                    else
                    {
                        MessageBox.Show("Aucune ligne modifiée. Vérifiez que l'utilisateur existe.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la modification de l'utilisateur : {ex.Message}");
                }
            }
        }

        // Ajouter un utilisateur à la base de données
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Veuillez entrer un nom d'utilisateur et un mot de passe.");
                return;
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Utilisateurs (Username, Password) VALUES (@username, @password)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    command.ExecuteNonQuery();
                    MessageBox.Show($"Utilisateur {username} ajouté avec succès !");

                    // Vider les champs après l'ajout
                    UsernameTextBox.Clear();
                    PasswordBox.Clear();

                    LoadUsersData(); // Recharge la liste des utilisateurs
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ajout de l'utilisateur : {ex.Message}");
                }
            }
        }

        // Supprimer un utilisateur de la base de données avec une confirmation
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as DataRowView;

            if (selectedUser == null)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur à supprimer.");
                return;
            }

            int userId = Convert.ToInt32(selectedUser["Id"]);

            // Afficher la boîte de dialogue de confirmation
            var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet utilisateur ?",
                                         "Confirmation de suppression",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "DELETE FROM Utilisateurs WHERE Id = @id";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@id", userId);

                        command.ExecuteNonQuery();
                        MessageBox.Show("Utilisateur supprimé avec succès !");
                        LoadUsersData(); // Recharge la liste des utilisateurs
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la suppression de l'utilisateur : {ex.Message}");
                    }
                }
            }
        }
    }
}
    