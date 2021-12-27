using System.IO;


namespace ImageIdentificationUI.UI.FileEntities
{
    public class FileViewModel : FileEntityViewModel
    {
        public FileViewModel(string name) : base(name) { }

        public FileViewModel(FileInfo fileInfo) : base(fileInfo.Name) => 
            FullName = fileInfo.FullName;
    }
}
