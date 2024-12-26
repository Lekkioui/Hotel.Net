using System;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace GestionHotel
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=localhost;Database=HotelLib;Uid=root;Pwd=;";
        private bool isPasswordVisible = false; 

        public MainWindow()
        {
            InitializeComponent();
        }

        
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = isPasswordVisible ? txtPasswordVisible.Text.Trim() : pwdPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Veuillez entrer un nom d'utilisateur et un mot de passe.");
                return;
            }

            using (MySqlConnection cn = new MySqlConnection(connectionString))
            {
                try
                {
                    cn.Open();

                    string query = "SELECT COUNT(*) FROM utilisateurs WHERE username = @username AND password = @password";
                    using (MySqlCommand cmd = new MySqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password); // À remplacer par un mot de passe hashé en production

                        int userCount = Convert.ToInt32(cmd.ExecuteScalar());
                        if (userCount > 0)
                        {
                            MessageBox.Show("Connexion réussie !");
                            
                            dashboardWindow dashboard = new dashboardWindow(username); 
                            dashboard.Show();

                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Nom d'utilisateur ou mot de passe incorrect.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur de connexion à la base de données : " + ex.Message);
                }
            }
        }

        // Bouton "Afficher/Masquer le mot de passe"
        private void btnShowHide_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                // Afficher le mot de passe
                txtPasswordVisible.Text = pwdPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                pwdPassword.Visibility = Visibility.Collapsed;
                btnShowHide.Content = "🙈"; // Icône pour masquer
            }
            else
            {
                // Masquer le mot de passe
                pwdPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                pwdPassword.Visibility = Visibility.Visible;
                btnShowHide.Content = "👁️"; // Icône pour afficher
            }
        }
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
