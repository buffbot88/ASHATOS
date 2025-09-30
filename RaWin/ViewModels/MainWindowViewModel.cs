using System.Collections.ObjectModel;
using System.Linq;
using RaCore.Engine.Manager;

namespace RaWin.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<ModulePanelViewModel> CoreModules { get; }

        public MainWindowViewModel(ModuleManager manager)
        {
            CoreModules = new ObservableCollection<ModulePanelViewModel>(
                manager.Modules
                    .Where(mw => mw.Instance != null &&
                        mw.Instance.GetType().GetCustomAttributes(typeof(RaModuleAttribute), true)
                            .Cast<RaModuleAttribute>().Any(attr => attr.Category == "core"))
                    .Select(mw => new ModulePanelViewModel(new ModuleWrapperView(mw), manager))
            );
        }
    }
}
