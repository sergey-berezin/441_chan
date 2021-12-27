using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ImageIdentificationUI.UI.Commands
{
    public interface IAsyncCommand : ICommand, INotifyPropertyChanged
    {
        Task ExecuteAsync(object parameter);
    }
}
