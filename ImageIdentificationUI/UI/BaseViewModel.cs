﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ImageIdentificationUI.UI
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


    }
}
