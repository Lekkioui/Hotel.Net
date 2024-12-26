using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GestionHotel
{
    public partial class ContractsPage : Page
    {
        private ObservableCollection<string> contractsList;

        public ContractsPage()
        {
            InitializeComponent();
            LoadContracts();
        }

        private void LoadContracts()
        {
            string contractsFolder = "C:\\Users\\MOI\\source\\repos\\GestionHotel\\bin\\Debug\\net8.0-windows\\Contracts"; // Remplace par le chemin réel
            if (Directory.Exists(contractsFolder))
            {
                contractsList = new ObservableCollection<string>(Directory.GetFiles(contractsFolder, "*.pdf").Select(file => Path.GetFileName(file)));
                ContractsListBox.ItemsSource = contractsList;
            }
            else
            {
                MessageBox.Show("Le dossier des contrats est introuvable.");
            }
        }

        private void OpenContractButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = (sender as Button)?.DataContext as string;
            if (!string.IsNullOrEmpty(fileName))
            {
                string filePath = Path.Combine("C:\\Users\\MOI\\source\\repos\\GestionHotel\\bin\\Debug\\net8.0-windows\\Contracts", fileName); // Remplace par ton chemin
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Fichier introuvable.");
                }
            }
        }

        private void DeleteContractButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = (sender as Button)?.DataContext as string;
            if (!string.IsNullOrEmpty(fileName))
            {
                string filePath = Path.Combine("C:\\Users\\MOI\\source\\repos\\GestionHotel\\bin\\Debug\\net8.0-windows\\Contracts", fileName); // Remplace par ton chemin
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        contractsList.Remove(fileName); // Retirer l'élément de l'ObservableCollection
                        MessageBox.Show("Contrat supprimé avec succès.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la suppression du contrat : {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Fichier introuvable.");
                }
            }
        }
    }
}
