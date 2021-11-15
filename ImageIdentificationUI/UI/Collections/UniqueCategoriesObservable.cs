using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace ImageIdentificationUI.UI.Collections
{
    public class UniqueCategoriesObservable : SortedSet<string>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChange() =>
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }


}