using System.ComponentModel;
using System.Runtime.CompilerServices;
using RaCore.Engine.Manager;

namespace RaWin.ViewModels
{
    /// <summary>
    /// ViewModel for the Status card in Monitoring.
    /// </summary>
    public class StatusPanelViewModel : INotifyPropertyChanged
    {
        private string _status;
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private string _uptime;
        public string Uptime
        {
            get => _uptime;
            set { _uptime = value; OnPropertyChanged(); }
        }

        public StatusPanelViewModel(ModuleManager manager)
        {
            var core = manager.CoreModules.FirstOrDefault();
            if (core?.Instance != null)
            {
                Status = core.Instance.Process("status");
                Uptime = core.Instance.Process("uptime");
            }
            else
            {
                Status = "(no status available)";
                Uptime = "(no uptime available)";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
