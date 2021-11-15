using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageIdentificationUI.UI.FileEntities
{
    public class FileViewModel : FileEntityViewModel
    {
        public FileViewModel(string name) : base(name) { }

        public FileViewModel(FileInfo fileInfo) : base(fileInfo.Name) =>
            FullName = fileInfo.FullName;
    }
}
