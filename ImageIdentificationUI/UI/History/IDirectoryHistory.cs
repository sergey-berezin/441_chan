using System;
using System.Collections.Generic;
using System.ComponentModel;
using ImageIdentificationUI.UI.FileEntities;
using System.Text;

namespace ImageIdentificationUI.UI.History
{
    interface IDirectoryHistory : INotifyPropertyChanged
    {
        bool CanMoveBack { get; }
        bool CanMoveForward { get; }
        DirectoryNode CurrentDirectory { get; }

        void MoveBack();
        void MoveForward();
        void Add(DirectoryViewModel directory);
    }
}
