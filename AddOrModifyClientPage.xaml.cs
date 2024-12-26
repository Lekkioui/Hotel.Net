using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GestionHotel
{
    public partial class AddOrModifyClientPage : Page
    {
        private string connectionString = "Server=localhost;Database=HotelLib;Uid=root;Pwd=;";
        private int? clientId;  // Nullable pour gérer l'ajout ou la modification
        private Action reloadClientsData; // Action pour recharger les données

        // Constructeur pour l'ajout de client
        public AddOrModifyClientPage(int? clientId, Action reloadClientsData)
        {
            InitializeComponent();
            this.clientId = clientId;
            this.reloadClientsData = reloadClientsData;

            // Si l'ID est null, c'est un ajout, sinon, c'est une modification
            if (clientId.HasValue)
            {
                LoadClientData(clientId.Value);
            }
        }

        // Charger les données d'un client existant
        private void LoadClientData(int clientId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Clients WHERE clientsID = @id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", clientId);
                    MySqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        PrenomTextBox.Text = reader["Prenom"].ToString();
                        NomTextBox.Text = reader["Nom"].ToString();
                        EmailTextBox.Text = reader["Email"].ToString();
                        TelephoneTextBox.Text = reader["Telephone"].ToString();
                        AdresseTextBox.Text = reader["Adresse"].ToString();
                        // Remplacer "DateNaissance" par "date_naissance" pour la correspondance avec la base de données
                        DateNaissancePicker.SelectedDate = reader["date_naissance"] != DBNull.Value ? (DateTime?)reader["date_naissance"] : null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}");
                }
            }
        }

        // Enregistrer ou modifier un client
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string prenom = PrenomTextBox.Text;
            string nom = NomTextBox.Text;
            string email = EmailTextBox.Text;
            string telephone = TelephoneTextBox.Text;
            string adresse = AdresseTextBox.Text;
            DateTime? dateNaissance = DateNaissancePicker.SelectedDate;

            if (string.IsNullOrEmpty(prenom) || string.IsNullOrEmpty(nom) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.");
                return;
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query;

                    if (clientId.HasValue) // Modification
                    {
                        // Remplacer "DateNaissance" par "date_naissance" dans la requête
                        query = "UPDATE Clients SET Prenom = @prenom, Nom = @nom, Email = @email, Telephone = @telephone, Adresse = @adresse, date_naissance = @dateNaissance WHERE clientsID = @id";
                    }
                    else // Ajout
                    {
                        // Remplacer "DateNaissance" par "date_naissance" dans la requête
                        query = "INSERT INTO Clients (Prenom, Nom, Email, Telephone, Adresse, date_naissance) VALUES (@prenom, @nom, @email, @telephone, @adresse, @dateNaissance)";
                    }

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@prenom", prenom);
                    command.Parameters.AddWithValue("@nom", nom);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@telephone", telephone);
                    command.Parameters.AddWithValue("@adresse", adresse);
                    // Ajouter la gestion de la date, en utilisant DBNull si aucune date n'est sélectionnée
                    command.Parameters.AddWithValue("@dateNaissance", (object)dateNaissance ?? DBNull.Value);

                    if (clientId.HasValue) command.Parameters.AddWithValue("@id", clientId.Value);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Client enregistré avec succès !");
                    reloadClientsData();
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'enregistrement du client : {ex.Message}");
                }
            }
        }

        // Annuler l'ajout/modification
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack(); // Retourner à la page précédente
        }
    }
}
