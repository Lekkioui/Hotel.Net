using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Net.Mail;
using System.Net;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Reflection.Metadata;
using System.Windows.Documents;
using static GestionHotel.AddOrModifyReservationPage;
using iTextSharp.text;

using System.Net.NetworkInformation;

using Paragraph = iTextSharp.text.Paragraph; // Alias pour éviter les conflits

namespace GestionHotel
{
    public partial class AddOrModifyReservationPage : Page
    {
        private string connectionString = "Server=localhost;Database=HotelLib;Uid=root;Pwd=;";
        private int? reservationId;
        private Action onSaveCallback;

        // Configuration email
        private const string SMTP_SERVER = "smtp.gmail.com";
        private const int SMTP_PORT = 587;
        private const string SMTP_USERNAME = "anassepaco@gmail.com";
        private const string SMTP_PASSWORD = "mijy lunw ysmu soot";
        public class ClientItem
        {
            public int ClientsID { get; set; }
            public string NomComplet { get; set; }
            public string Email { get; set; }
        }

        public class ChambreItem
        {
            public int ChambreID { get; set; }
            public string InfoChambre { get; set; }
            public decimal PrixNuit { get; set; }
        }

        public AddOrModifyReservationPage(int? reservationId = null, Action onSaveCallback = null)
        {
            InitializeComponent();
            this.reservationId = reservationId;
            this.onSaveCallback = onSaveCallback;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ArrivalDatePicker.SelectedDate = DateTime.Today;
            DepartureDatePicker.SelectedDate = DateTime.Today.AddDays(1);

            LoadClients();

            // Events pour mettre à jour la liste des chambres disponibles
            ArrivalDatePicker.SelectedDateChanged += (s, ev) => LoadChambres();
            DepartureDatePicker.SelectedDateChanged += (s, ev) => LoadChambres();

            if (reservationId.HasValue)
            {
                LoadReservationData(reservationId.Value);
            }
        }

        private void LoadClients()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT ClientsID, 
                                          CONCAT(nom, ' ', prenom) as NomComplet,
                                          email 
                                   FROM Clients 
                                   ORDER BY nom, prenom";
                    var command = new MySqlCommand(query, connection);
                    var reader = command.ExecuteReader();
                    var clients = new List<ClientItem>();

                    while (reader.Read())
                    {
                        clients.Add(new ClientItem
                        {
                            ClientsID = Convert.ToInt32(reader["ClientsID"]),
                            NomComplet = reader["NomComplet"].ToString(),
                            Email = reader["email"].ToString()
                        });
                    }

                    ClientComboBox.ItemsSource = clients;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des clients : {ex.Message}");
                }
            }
        }

        private void LoadChambres()
        {
            // Vérifier que les deux dates sont sélectionnées
            if (!ArrivalDatePicker.SelectedDate.HasValue || !DepartureDatePicker.SelectedDate.HasValue)
                return; // Ne pas charger les chambres si les dates ne sont pas valides

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
            SELECT DISTINCT c.ChambreID, 
                   CONCAT('Chambre ', c.numchambre, ' - ', c.typechambre) as InfoChambre,
                   c.Prix
            FROM Chambres c
            WHERE NOT EXISTS (
                SELECT 1 FROM Reservations r 
                WHERE r.ChambreID = c.ChambreID 
                AND (@DateDepart > r.DateArrivee AND @DateArrivee < r.DateDepart)
                AND r.StatutReservation != 'Annulée'
                AND r.ReservationID != COALESCE(@ReservationID, -1)
            )
            ORDER BY c.numchambre";

                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@DateArrivee", ArrivalDatePicker.SelectedDate.Value);
                    command.Parameters.AddWithValue("@DateDepart", DepartureDatePicker.SelectedDate.Value);
                    command.Parameters.AddWithValue("@ReservationID", reservationId ?? (object)DBNull.Value);

                    var reader = command.ExecuteReader();
                    var chambres = new List<ChambreItem>();

                    while (reader.Read())
                    {
                        chambres.Add(new ChambreItem
                        {
                            ChambreID = Convert.ToInt32(reader["ChambreID"]),
                            InfoChambre = reader["InfoChambre"].ToString(),
                            PrixNuit = Convert.ToDecimal(reader["Prix"])
                        });
                    }

                    // Mettre à jour l'élément ComboBox avec les chambres disponibles
                    RoomComboBox.ItemsSource = chambres;

                    // Si nous modifions une réservation, sélectionner la chambre déjà attribuée
                    if (reservationId.HasValue)
                    {
                        LoadReservationData(reservationId.Value);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des chambres : {ex.Message}");
                }
            }
        }



        private void LoadReservationData(int reservationId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Reservations WHERE ReservationID = @id";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", reservationId);
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int clientId = Convert.ToInt32(reader["ClientsID"]);
                        int chambreId = Convert.ToInt32(reader["ChambreID"]);

                        ClientComboBox.SelectedValue = clientId;
                        RoomComboBox.SelectedValue = chambreId;  // Cette ligne pré-sélectionne la chambre dans le ComboBox
                        ArrivalDatePicker.SelectedDate = Convert.ToDateTime(reader["DateArrivee"]);
                        DepartureDatePicker.SelectedDate = Convert.ToDateTime(reader["DateDepart"]);

                        string status = reader["StatutReservation"].ToString();
                        foreach (ComboBoxItem item in StatusComboBox.Items)
                        {
                            if (item.Content.ToString() == status)
                            {
                                StatusComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}");
                }
            }
        }


        private bool ValidateInputs()
        {
            if (ClientComboBox.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un client.");
                return false;
            }

            if (RoomComboBox.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une chambre.");
                return false;
            }

            if (!ArrivalDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Veuillez sélectionner une date d'arrivée.");
                return false;
            }

            if (!DepartureDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Veuillez sélectionner une date de départ.");
                return false;
            }

            if (DepartureDatePicker.SelectedDate <= ArrivalDatePicker.SelectedDate)
            {
                MessageBox.Show("La date de départ doit être postérieure à la date d'arrivée.");
                return false;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un statut.");
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = reservationId.HasValue
                        ? "UPDATE Reservations SET ClientsID = @ClientsID, ChambreID = @ChambreID, DateArrivee = @DateArrivee, DateDepart = @DateDepart, StatutReservation = @StatutReservation WHERE ReservationID = @ReservationID"
                        : "INSERT INTO Reservations (ClientsID, ChambreID, DateArrivee, DateDepart, StatutReservation) VALUES (@ClientsID, @ChambreID, @DateArrivee, @DateDepart, @StatutReservation); SELECT LAST_INSERT_ID();";

                    var command = new MySqlCommand(query, connection);
                    var selectedClient = (ClientItem)ClientComboBox.SelectedItem;
                    var selectedRoom = (ChambreItem)RoomComboBox.SelectedItem;
                    var selectedStatus = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();

                    command.Parameters.AddWithValue("@ClientsID", selectedClient.ClientsID);
                    command.Parameters.AddWithValue("@ChambreID", selectedRoom.ChambreID);
                    command.Parameters.AddWithValue("@DateArrivee", ArrivalDatePicker.SelectedDate.Value);
                    command.Parameters.AddWithValue("@DateDepart", DepartureDatePicker.SelectedDate.Value);
                    command.Parameters.AddWithValue("@StatutReservation", selectedStatus);

                    int currentReservationId;
                    if (reservationId.HasValue)
                    {
                        command.Parameters.AddWithValue("@ReservationID", reservationId.Value);
                        command.ExecuteNonQuery();
                        currentReservationId = reservationId.Value;
                    }
                    else
                    {
                        currentReservationId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    if (selectedStatus == "Confirmée")
                    {
                        // Générer et envoyer le contrat
                        string pdfPath = GenerateContract(currentReservationId, selectedClient, selectedRoom);
                        SendContractByEmail(selectedClient, selectedClient.Email, pdfPath, currentReservationId);

                    }

                    MessageBox.Show("Réservation enregistrée avec succès !");
                    NavigationService.Navigate(new ReservationsPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'enregistrement : {ex.Message}");
                }
            }
        }


        private string GenerateContract(int reservationId, ClientItem client, ChambreItem chambre)
        {
            // Définir le chemin du fichier PDF
            string pdfPath = $"Contracts/reservation_{reservationId}.pdf";
            Directory.CreateDirectory("Contracts"); // Créer le dossier s'il n'existe pas

            // Créer et écrire dans le fichier PDF
            using (FileStream fs = new FileStream(pdfPath, FileMode.Create))
            {
                using (iTextSharp.text.Document document = new iTextSharp.text.Document())
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    document.Open();

                    // Ajouter un logo ou image (si nécessaire)
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("C:\\Users\\MOI\\Pictures\\Saved Pictures\\Logo_Hotel.png");
                    logo.ScaleToFit(100f, 100f);
                    logo.SetAbsolutePosition(document.PageSize.Width - 110f, document.PageSize.Height - 120f);
                    document.Add(logo);


                    // Titre du contrat
                    Font headerFont = new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD, BaseColor.BLACK);
                    Paragraph title = new Paragraph("CONTRAT DE RÉSERVATION", headerFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20f;
                    document.Add(title);

                    // Informations de réservation
                    Font infoFont = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL);
                    Paragraph reservationInfo = new Paragraph($"Réservation N° {reservationId}", infoFont)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    reservationInfo.SpacingAfter = 10f;
                    document.Add(reservationInfo);

                    Paragraph dateInfo = new Paragraph($"Date de création: {DateTime.Now:dd/MM/yyyy}", infoFont)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    dateInfo.SpacingAfter = 30f;
                    document.Add(dateInfo);

                    // Ligne de séparation
                    LineBreak separator = new LineBreak();
                    //document.Add(separator);1f, 100f, BaseColor.GRAY, Element.ALIGN_CENTER, -1

                    // Informations client
                    Font sectionHeaderFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD);
                    Paragraph clientTitle = new Paragraph("INFORMATIONS CLIENT", sectionHeaderFont);
                    clientTitle.SpacingBefore = 20f;
                    clientTitle.SpacingAfter = 10f;
                    document.Add(clientTitle);

                    // Tableau pour les informations du client
                    PdfPTable clientTable = new PdfPTable(2) { WidthPercentage = 100 };
                    clientTable.SetWidths(new float[] { 1f, 3f });
                    clientTable.AddCell(new Phrase("Nom complet:"));
                    clientTable.AddCell(new Phrase(client.NomComplet, infoFont));
                    clientTable.AddCell(new Phrase("Email:"));
                    clientTable.AddCell(new Phrase(client.Email, infoFont));
                    document.Add(clientTable);

                    // Ligne de séparation
                    //document.Add(separator);
                    // Détails de la réservation
                    Paragraph reservationDetailsTitle = new Paragraph("DÉTAILS DE LA RÉSERVATION", sectionHeaderFont);
                    reservationDetailsTitle.SpacingBefore = 20f;
                    reservationDetailsTitle.SpacingAfter = 10f;
                    document.Add(reservationDetailsTitle);

                    // Tableau pour les détails de la réservation
                    PdfPTable reservationTable = new PdfPTable(2) { WidthPercentage = 100 };
                    reservationTable.SetWidths(new float[] { 1f, 3f });
                    reservationTable.AddCell(new Phrase("Chambre:"));
                    reservationTable.AddCell(new Phrase(chambre.InfoChambre, infoFont));
                    reservationTable.AddCell(new Phrase("Date d'arrivée:"));
                    reservationTable.AddCell(new Phrase(ArrivalDatePicker.SelectedDate.Value.ToString("dd/MM/yyyy"), infoFont));
                    reservationTable.AddCell(new Phrase("Date de départ:"));
                    reservationTable.AddCell(new Phrase(DepartureDatePicker.SelectedDate.Value.ToString("dd/MM/yyyy"), infoFont));
                    document.Add(reservationTable);

                    // Calcul du prix total
                    int nombreNuits = (DepartureDatePicker.SelectedDate.Value - ArrivalDatePicker.SelectedDate.Value).Days;
                    decimal prixTotal = nombreNuits * chambre.PrixNuit;
                    document.Add(new Paragraph($"Nombre de nuits: {nombreNuits}", infoFont));
                    document.Add(new Paragraph($"Prix par nuit: {chambre.PrixNuit:C2}", infoFont));
                    document.Add(new Paragraph($"Prix total: {prixTotal:C2}", infoFont));

                    // Ligne de séparation
                    //document.Add(separator);

                    // Ajout d'une section pour les conditions générales (facultatif)
                    Paragraph terms = new Paragraph("Conditions générales", sectionHeaderFont)
                    {
                        SpacingBefore = 20f,
                        SpacingAfter = 10f
                    };

                    terms.Add(new Paragraph("\n1. **Conditions de réservation**\n" +
                        "   - Une réservation est considérée comme confirmée après réception d'un acompte de 30% du montant total du séjour.\n" +
                        "   - Le solde restant doit être réglé à l'arrivée à l'hôtel.\n" +
                        "   - Les réservations doivent être effectuées au moins 48 heures avant l'arrivée.\n\n" +

                        "2. **Annulation de réservation**\n" +
                        "   - En cas d'annulation effectuée plus de 7 jours avant la date d'arrivée, l'acompte sera remboursé intégralement.\n" +
                        "   - En cas d'annulation effectuée moins de 7 jours avant la date d'arrivée, l'acompte sera non remboursable.\n" +
                        "   - Si le client ne se présente pas à l'hôtel sans prévenir, la totalité du montant du séjour sera facturée.\n\n" +

                        "3. **Heure d'arrivée et de départ**\n" +
                        "   - L'heure d'arrivée est fixée à partir de 14h00. Les chambres sont disponibles à partir de cette heure-là.\n" +
                        "   - L'heure de départ est fixée à 12h00. Un supplément pourra être demandé pour les départs après cet horaire.\n\n" +

                        "4. **Règles de l'hôtel**\n" +
                        "   - Il est interdit de fumer dans toutes les chambres de l'hôtel.\n" +
                        "   - Les animaux ne sont pas autorisés dans l'établissement, sauf accord préalable de la direction.\n" +
                        "   - Le respect des horaires de tranquillité (22h00 - 8h00) est demandé pour le confort de tous.\n\n" +

                        "5. **Responsabilité**\n" +
                        "   - L'hôtel ne pourra être tenu responsable des pertes ou des vols d'objets personnels dans les chambres ou dans les parties communes de l'établissement.\n" +
                        "   - Le client est responsable de tout dommage causé à la chambre ou à l'hôtel durant son séjour.\n\n" +

                        "6. **Modification des conditions**\n" +
                        "   - L'hôtel se réserve le droit de modifier ces conditions générales à tout moment. Les clients seront informés de toute modification avant leur réservation.\n\n" +

                        "7. **Force majeure**\n" +
                        "   - L'hôtel ne pourra être tenu responsable en cas d'événements imprévus et indépendants de sa volonté, tels que des catastrophes naturelles, des grèves, des pandémies, etc." +
                        "\n\n"));

                    document.Add(terms);

                    Paragraph termsText = new Paragraph("Les conditions générales de la réservation s'appliquent. Merci de les lire attentivement.", infoFont);
                    document.Add(termsText);

                    // Signature
                    Paragraph signature = new Paragraph("\nSignature du client: ___________________", infoFont)
                    {
                        Alignment = Element.ALIGN_RIGHT
                    };
                    document.Add(signature);

                    // Clôture du document
                    document.Close();
                }
            }

            return pdfPath; // Retourner le chemin du fichier PDF généré
        }


        private void SendContractByEmail(ClientItem client, string clientEmail, string pdfPath, int reservationId)
        {
            try
            {
                // Création de l'instance MailMessage
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(SMTP_USERNAME);
                mail.To.Add(clientEmail);
                mail.Subject = $"Confirmation de votre réservation N°{reservationId}";

                // Corps du message
                mail.Body = $"Bonjour M./Mme {client.NomComplet},\n\n" +
                            "Nous vous remercions sincèrement d'avoir choisi notre hôtel pour votre prochain séjour. " +
                            "Nous sommes ravis de vous accueillir et nous nous engageons à rendre votre séjour aussi agréable et confortable que possible.\n\n" +
                            "Veuillez trouver ci-joint votre contrat de réservation pour la confirmation de votre séjour. " +
                            "N'hésitez pas à nous contacter si vous avez la moindre question ou besoin d'assistance supplémentaire.\n\n" +
                            "Nous vous souhaitons un excellent séjour et espérons que vous passerez de merveilleuses nuits parmi nous.\n\n" +
                            "Cordialement,\n" +
                            "[Anasse's Hotel]\n" +
                            "[Hay Elbahja, Marrakech]";



                // Attacher le fichier PDF
                mail.Attachments.Add(new Attachment(pdfPath));

                // Configuration du client SMTP
                SmtpClient smtpClient = new SmtpClient(SMTP_SERVER, SMTP_PORT)
                {
                    Credentials = new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD),
                    EnableSsl = true
                };

                // Envoi de l'email
                smtpClient.Send(mail);

                // Affichage d'un message de confirmation
                MessageBox.Show("L'email a été envoyé avec succès.", "Succès", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                // Gestion des erreurs
                MessageBox.Show($"Erreur lors de l'envoi de l'email : {ex.Message}", "Erreur", MessageBoxButton.OK);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ReservationsPage());
        }
        // Gestion du changement de la date d'arrivée
        private void ArrivalDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Vérifiez si une date est sélectionnée
            if (ArrivalDatePicker.SelectedDate.HasValue)
            {
                // Mettre à jour la liste des chambres disponibles
                LoadChambres();
            }
        }

        // Gestion du changement de la date de départ
        private void DepartureDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Vérifiez si une date est sélectionnée
            if (DepartureDatePicker.SelectedDate.HasValue)
            {
                // Mettre à jour la liste des chambres disponibles
                LoadChambres();
            }
        }


    }
}