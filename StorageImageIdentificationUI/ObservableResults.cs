using System.Collections.Generic;
using System.Collections.Specialized;


namespace StorageImageIdentificationUI
{
    public partial class MainWindow
    {
        public class ObservableResults : List<PictureInfo>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public new void Add(PictureInfo pictureInfo)
            {
                base.Add(pictureInfo);
                OnCollectionChanged();
            }            

            public new void Clear()
            {
                base.Clear();
                OnCollectionChanged();
            }

 
            public void OnCollectionChanged() => 
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
