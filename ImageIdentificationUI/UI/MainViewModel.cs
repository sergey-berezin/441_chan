using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Threading;
using System.ComponentModel;
using ImageIdentificationUI.UI.Commands;
using ImageIdentificationUI.UI.History;
using ImageIdentificationUI.UI.FileEntities;
using ImageIdentificationUI.UI.Collections;
using YOLOv4;


namespace ImageIdentificationUI.UI
{
    public class MainViewModel : BaseViewModel
    {
        private Result _extendedInfo = null;

        public MainViewModel()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            History = new DirectoryHistory("Please select the path");
            ImagesList = new ImagesCollection();
            RecognitionResults = new Dictionary<string, Result>();
            SpecifiedCategories = new UniqueCategoriesObservable();

            EntitiesList = new ObservableCollection<FileEntityViewModel>();

            StartProcess = new AsyncCommand(Start, CanStart);
            CancelProcess = new AsyncCommand(Cancel, CanCancel);
            ShowExtendedInfo = new AsyncCommand(ShowExtraInfo, CanShowExtraInfo);

            StartProcess.CanExecuteChanged += CanCancelChanged;
            ImagesList.PropertyChanged += CanStartChanged;
        }


        public ObservableCollection<FileEntityViewModel> EntitiesList { get; set; }

        public Dispatcher Dispatcher { get; set; }

        public DirectoryHistory History { get; }

        public ImagesCollection ImagesList { get; set; }

        public Dictionary<string, Result> RecognitionResults { get; set; }

        public Result ExtendedInfo
        {
            get => _extendedInfo;
            set
            {
                if (_extendedInfo != value)
                {
                    _extendedInfo = value;
                    OnPropertyChanged(nameof(ExtendedInfo));
                }
            }
        }

        public UniqueCategoriesObservable SpecifiedCategories { get; set; }


        public AsyncCommand StartProcess { get; }

        public AsyncCommand CancelProcess { get; }

        public AsyncCommand ShowExtendedInfo { get; }

        public AsyncCommand SelectCategory { get; }


        private async void Start(object parameter)
        {
            Processing processing = new Processing();
            string imagesPath = History.CurrentDirectory.CurrentNode.FullName;
            await foreach (var Result in processing.ProcessImagesAsync(imagesPath))
            {
                Dispatcher?.Invoke(() =>
                {
                    ImagesList[Result.ImageName].SourceChanged(Result.Bitmap);
                    if (StartProcess.IsCanceled)
                        processing.Cancel();

                    RecognitionResults[Result.ImageName] = Result;
                });
            }
        }

        private bool CanStart(object parameter) => !ImagesList.IsEmpty;


        private void Cancel(object parameter)
        {
            Dispatcher?.Invoke(() => { StartProcess.Cancel(); });
        }

        private bool CanCancel(object parameter) => StartProcess.IsExecution;


        private void ShowExtraInfo(object parameter)
        {
            if (RecognitionResults.Count > 0)
            {
                var image = (KeyValuePair<string, ImageViewModel>)parameter;
                var imageName = image.Key;
                ExtendedInfo = RecognitionResults[imageName];
            }
        }

        private bool CanShowExtraInfo(object parameter) => parameter != null;


        public bool SelectSpecified(object argsItem)
        {
            var item = (KeyValuePair<string, ImageViewModel>)argsItem;
            bool result = true;
            if (SpecifiedCategories != null && SpecifiedCategories.Count > 0)
            {
                result = false;
                foreach (string category in SpecifiedCategories)
                {
                    if (RecognitionResults.ContainsKey(item.Key) &&
                        RecognitionResults[item.Key].Categories.Select(cat => cat.ObjName).Contains(category))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }


        public void OpenDirectory(DirectoryViewModel directory)
        {
            EntitiesList.Clear();
            ImagesList.Clear();
            RecognitionResults.Clear();
            ExtendedInfo = null;

            foreach (var directoryName in Directory.GetDirectories(directory.FullName))
                EntitiesList.Add(new DirectoryViewModel(new DirectoryInfo(directoryName)));
            foreach (var fileName in Directory.GetFiles(directory.FullName))
            {
                var fileInfo = new FileInfo(fileName);
                var extensions = new ObservableCollection<string>() { ".png", ".jpg" };
                if (extensions.Contains(fileInfo.Extension))
                {
                    var img = new ImageViewModel(fileInfo);
                    ImagesList.Add(fileInfo.Name, img);
                    OnPropertyChanged(nameof(ImagesList));
                    EntitiesList.Add(img);
                }
                else
                    EntitiesList.Add(new FileViewModel(fileInfo));
            }
        }

        private void CanCancelChanged(object sender, EventArgs e) =>
            Dispatcher?.Invoke(() => { CancelProcess?.RaiseCanExecuteChanged(); });

        private void CanStartChanged(object sender, PropertyChangedEventArgs e) =>
            Dispatcher?.Invoke(() => { StartProcess?.RaiseCanExecuteChanged(); });

    }
}