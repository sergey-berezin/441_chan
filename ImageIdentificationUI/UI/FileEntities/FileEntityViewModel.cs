using System;
using System.Collections.Generic;
using System.Text;

namespace ImageIdentificationUI.UI.FileEntities
{
    public abstract class FileEntityViewModel : BaseViewModel
    {
        protected FileEntityViewModel(string name) => Name = name;

        public string Name { get; set; }

        public string FullName { get; set; }
    }
}
