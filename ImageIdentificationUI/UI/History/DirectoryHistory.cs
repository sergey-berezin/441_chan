using System;
using System.Collections.Generic;
using System.ComponentModel;
using ImageIdentificationUI.UI.FileEntities;
using System.Text;

namespace ImageIdentificationUI.UI.History
{
    public class DirectoryHistory : IDirectoryHistory
    {

        private readonly DirectoryNode _head;

        private DirectoryNode _currentDirectory;


        public DirectoryHistory(string headName)
        {
            _head = new DirectoryNode(headName);
            CurrentDirectory = _head;
        }


        public bool CanMoveBack => CurrentDirectory.PrevNode != null;

        public bool CanMoveForward => CurrentDirectory.NextNode != null;

        public DirectoryNode CurrentDirectory
        {
            get => _currentDirectory;
            private set
            {
                if (_currentDirectory != value)
                {
                    _currentDirectory = value;
                    RaiseHistoryChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public void Add(DirectoryViewModel directory)
        {
            var newNode = new DirectoryNode(directory);
            newNode.PrevNode = CurrentDirectory;
            CurrentDirectory.NextNode = newNode;

            MoveForward();
        }

        public void MoveBack() => CurrentDirectory = CurrentDirectory.PrevNode;

        public void MoveForward() => CurrentDirectory = CurrentDirectory.NextNode;

        public bool IsOnRoot() => CurrentDirectory.CurrentNode.FullName == _head.CurrentNode.FullName;

        public void SimulateHistoryChanged() => RaiseHistoryChanged();


        private void RaiseHistoryChanged() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDirectory)));

    }
}