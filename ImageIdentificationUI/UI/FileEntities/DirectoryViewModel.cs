using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageIdentificationUI.UI.FileEntities
{
    public class DirectoryViewModel : FileEntityViewModel
    {
        public DirectoryViewModel(string name) : base(name) { }

        public DirectoryViewModel(DirectoryInfo directoryInfo) : base(directoryInfo.Name) =>
            FullName = directoryInfo.FullName;

    }
}