using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Threading;
using System.ComponentModel;
using ImageIdentificationUI.UI.Commands;
using ImageIdentificationUI.UI.FileEntities;
using ImageIdentificationUI.UI.Collections;
using YOLOv4;


namespace ImageIdentificationUI.UI
{
    public class MainViewModel : BaseViewModel
    {
        private Result _extendedInfo = null;
        public string Path;

        public MainViewModel()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            ImagesList = new ImagesCollection();
            RecognitionResults = new Dictionary<string, Result>();

            StartProcess = new AsyncCommand(Start, CanStart);
            CancelProcess = new AsyncCommand(Cancel, CanCancel);
            ShowExtendedInfo = new AsyncCommand(ShowExtraInfo, CanShowExtraInfo);

            StartProcess.CanExecuteChanged += CanCancelChanged;
            ImagesList.PropertyChanged += CanStartChanged;
        }

        public Dispatcher Dispatcher { get; set; }

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


        public AsyncCommand StartProcess { get; }

        public AsyncCommand CancelProcess { get; }

        public AsyncCommand ShowExtendedInfo { get; }


        private async void Start(object parameter)
        {
            Processing processing = new Processing();
            await foreach (var Result in processing.ProcessImagesAsync(Path))
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

        public void OpenDirectory(string Path)
        {
            ImagesList.Clear();
            RecognitionResults.Clear();
            ExtendedInfo = null;

            foreach (var fileName in Directory.GetFiles(Path))
            {
                var fileInfo = new FileInfo(fileName);
                var extensions = new ObservableCollection<string>() { ".png", ".jpg" };
                if (extensions.Contains(fileInfo.Extension))
                {
                    var img = new ImageViewModel(fileInfo);
                    ImagesList.Add(fileInfo.Name, img);
                    OnPropertyChanged(nameof(ImagesList));
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("No picture file is found in the directory, please select the directory again!", "Error");
                    return;
                }
            }
        }

        private void CanCancelChanged(object sender, EventArgs e) =>
            Dispatcher?.Invoke(() => { CancelProcess?.RaiseCanExecuteChanged(); });

        private void CanStartChanged(object sender, PropertyChangedEventArgs e) =>
            Dispatcher?.Invoke(() => { StartProcess?.RaiseCanExecuteChanged(); });

    }
}