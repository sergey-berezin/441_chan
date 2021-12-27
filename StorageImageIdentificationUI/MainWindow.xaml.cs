using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using Ookii.Dialogs.Wpf;
using YOLOv4;

namespace StorageImageIdentificationUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Processing processing = new();
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private string folderPath = "Please select the folder path:";

        public MainWindow()
        {
            Results = new();
            UniqueCategories = new();
            InitializeComponent();
            DataContext = this;

            SelectFolder.IsEnabled = true;
            Start.IsEnabled = false;
            Cancel.IsEnabled = false;
            OpenStorage.IsEnabled = false;
            Delete.IsEnabled = true;

            LoadData();
        }

        public ObservableResults Results { get; }
        public ObservableCollection<string> UniqueCategories { get; }

        public string FolderPath
        {
            get => folderPath;
            set
            {
                if (value != folderPath)
                {
                    folderPath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderPath)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            SelectFolder.IsEnabled = false;
            Start.IsEnabled = false;
            Delete.IsEnabled = false;
            Cancel.IsEnabled = true;
            processing = new();
            Results.Clear();
            await Task.Factory.StartNew(() =>
            {
                foreach (var results in processing.ProcessImagesAsync(FolderPath))
                {
                    dispatcher?.Invoke(() =>
                    {
                        PictureInfo picInfo = new(results);
                        Results.Add(picInfo);
                        DbSession.WriteTo(picInfo);
                    });
                    foreach (var res in results.objectList)
                    {
                        if (!UniqueCategories.Contains(res.Label))
                        {
                            dispatcher?.Invoke(() =>
                            {
                                UniqueCategories.Add(res.Label);
                            });
                        }
                    }
                }
            });

            SelectFolder.IsEnabled = true;
            OpenStorage.IsEnabled = true;
            Delete.IsEnabled = true;
            Cancel.IsEnabled = false;
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new();
            bool open = (bool)folderDialog.ShowDialog();
            if (open)
            {
                Start.IsEnabled = true;
                FolderPath = folderDialog.SelectedPath;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectFolder.IsEnabled = true;
            Start.IsEnabled = true;
            Cancel.IsEnabled = false;
            OpenStorage.IsEnabled = true;
            Delete.IsEnabled = true;
            processing?.Cancel();
        }

        private void LoadData()
        {
            foreach (var picInfo in DbSession.ReadFrom())
            {
                Results.Add(picInfo);
            }
        }

        private void OpenStorage_Click(object sender, RoutedEventArgs e)
        {
            SelectFolder.IsEnabled = true;
            Start.IsEnabled = false;
            Cancel.IsEnabled = false;
            OpenStorage.IsEnabled = false;
            SelectFolder.IsEnabled = false;
            Delete.IsEnabled = true;
            FolderPath = "";
            Results.Clear();
            UniqueCategories.Clear();
            LoadData();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ImageInf.SelectedItems.Count > 0)
            {
                foreach (var elem in ImageInf.SelectedItems)
                {
                    DbSession.RemoveFrom((PictureInfo)elem);
                    Results.Remove((PictureInfo)elem);
                }
                Results.OnCollectionChanged();
                Start.IsEnabled = true;
                Cancel.IsEnabled = false;
                OpenStorage.IsEnabled = false;
                SelectFolder.IsEnabled = true;
            }
        }
    }
}

