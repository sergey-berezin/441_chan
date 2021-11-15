using System;
using System.Collections.Generic;
using System.Text;
using ImageIdentificationUI.UI.FileEntities;

namespace ImageIdentificationUI.UI.History
{
    public class DirectoryNode
    {
        public DirectoryNode(string directoryName) =>
            CurrentNode = new DirectoryViewModel(directoryName) { FullName = directoryName };

        public DirectoryNode(DirectoryViewModel directory) => CurrentNode = directory;

        public DirectoryViewModel CurrentNode { get; set; }

        public DirectoryNode NextNode { get; set; }

        public DirectoryNode PrevNode { get; set; }


        public override string ToString() => CurrentNode.FullName;

    }
}