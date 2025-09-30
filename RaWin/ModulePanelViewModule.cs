using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using RaCore.Engine.Manager;

namespace RaWin
{
    public class MainWindowViewModel
    {
        public ObservableCollection<ModuleWrapperView> CoreModules { get; }

        public MainWindowViewModel(ModuleManager moduleManager)
        {
            foreach (var mw in moduleManager.Modules)
            {
                var attr = mw.Instance?.GetType().GetCustomAttribute<RaModuleAttribute>();
                System.Diagnostics.Debug.WriteLine($"Module: {mw.Instance?.Name} | Category: {attr?.Category}");
            }

            var coreModuleViews = moduleManager.Modules
                .Where(mw => mw.Instance != null &&
                    mw.Instance.GetType().GetCustomAttribute<RaModuleAttribute>()?.Category == "core")
                .Select(mw => new ModuleWrapperView(mw))
                .ToList();

            CoreModules = new ObservableCollection<ModuleWrapperView>(coreModuleViews);
        }
    }
}
