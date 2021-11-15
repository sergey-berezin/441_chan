using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text;

namespace ImageIdentificationUI.UI.Commands
{
    public interface IAsyncCommand : ICommand, INotifyPropertyChanged
    {
        Task ExecuteAsync(object parameter);
    }
}
