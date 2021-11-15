using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageIdentificationUI.UI;
using ImageIdentificationUI.UI.FileEntities;
using System.Windows.Forms;
using System.IO;

namespace ImageIdentificationUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainViewModel mainViewModel { get; set; }

        public MainWindow()
        {
            mainViewModel = new MainViewModel();
            InitializeComponent();
            DataContext = mainViewModel;
        }

        private void SelectOnlySpecified(object sender, FilterEventArgs args) =>
            args.Accepted = mainViewModel.SelectSpecified(args.Item);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Personal;
            folderBrowserDialog.ShowDialog();
            if (folderBrowserDialog.SelectedPath == string.Empty)
            {
                System.Windows.Forms.MessageBox.Show("Address not selected, please select again.", "Error");
                return;
            }
            DirectoryViewModel Path = new DirectoryViewModel(new DirectoryInfo(folderBrowserDialog.SelectedPath));
            mainViewModel.History.Add(Path);
            mainViewModel.OpenDirectory(Path);
        }
    }
}
