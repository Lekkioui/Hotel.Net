using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.Mail;
using System.Net;

namespace GestionHotel
{
    public partial class ReservationsPage : Page
    {
        private string connectionString = "Server=localhost;Database=HotelLib;Uid=root;Pwd=;";
        private const string SMTP_SERVER = "smtp.example.com";
        private const int SMTP_PORT = 587;
        private const string SMTP_USERNAME = "your-email@example.com";
        private const string SMTP_PASSWORD = "your-password";

        public ReservationsPage()
        {
            InitializeComponent();
            LoadReservationsData();
        }

        private void LoadReservationsData()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT r.ReservationID, r.DateArrivee, r.DateDepart, 
                                          c.nom AS ClientNom, ch.numchambre AS ChambreNumero 
                                   FROM reservations r 
                                   JOIN clients c ON r.ClientsID = c.ClientsID 
                                   JOIN chambres ch ON r.ChambreID = ch.ChambreID
                                   ORDER BY r.DateArrivee DESC";

                    var command = new MySqlCommand(query, connection);
                    var reader = command.ExecuteReader();
                    var reservations = new System.Collections.ObjectModel.ObservableCollection<Reservation>();

                    while (reader.Read())
                    {
                        reservations.Add(new Reservation
                        {
                            ReservationID = Convert.ToInt32(reader["ReservationID"]),
                            ClientNom = reader["ClientNom"].ToString(),
                            ChambreNumero = reader["ChambreNumero"].ToString(),
                            DateArrivee = Convert.ToDateTime(reader["DateArrivee"]),
                            DateDepart = Convert.ToDateTime(reader["DateDepart"])
                        });
                    }

                    ReservationsDataGrid.ItemsSource = reservations;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des réservations : {ex.Message}",
                                  "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddOrModifyReservationPage(null, () => {
                LoadReservationsData();
                NavigationService.Navigate(new ReservationsPage());
            }));
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedReservation = ReservationsDataGrid.SelectedItem as Reservation;
            if (selectedReservation != null)
            {
                NavigationService.Navigate(new AddOrModifyReservationPage(selectedReservation.ReservationID, () => {
                    LoadReservationsData();
                    NavigationService.Navigate(new ReservationsPage());
                }));
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une réservation à modifier.",
                              "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedReservation = ReservationsDataGrid.SelectedItem as Reservation;
            if (selectedReservation != null)
            {
                var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cette réservation ?",
                                           "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();
                            string query = "DELETE FROM reservations WHERE ReservationID = @ReservationID";
                            var command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@ReservationID", selectedReservation.ReservationID);
                            command.ExecuteNonQuery();

                            MessageBox.Show("Réservation supprimée avec succès !",
                                          "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadReservationsData();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erreur lors de la suppression de la réservation : {ex.Message}",
                                          "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une réservation à supprimer.",
                              "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public class Reservation
    {
        public int ReservationID { get; set; }
        public string ClientNom { get; set; }
        public string ChambreNumero { get; set; }
        public DateTime DateArrivee { get; set; }
        public DateTime DateDepart { get; set; }
    }
}