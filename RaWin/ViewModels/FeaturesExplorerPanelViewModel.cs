using System.Collections.ObjectModel;
using RaCore.Engine.Manager;
using RaWin;

namespace RaWin.ViewModels
{
    public class FeatureExplorerPanelViewModel : ObservableObject
    {
        private readonly IRaModule _featureExplorer;

        public string FeatureReport { get; private set; }
        public ObservableCollection<string> Modules { get; } = new();

        public FeatureExplorerPanelViewModel(ModuleManager manager)
        {
            _featureExplorer = manager.GetModuleByName("FeatureExplorer");
            LoadFeatureReport();
        }

        public void LoadFeatureReport()
        {
            if (_featureExplorer != null)
            {
                FeatureReport = _featureExplorer.Process("features full");
                // Optionally parse and add module names from the report
            }
            else
            {
                FeatureReport = "(FeatureExplorer module not loaded)";
            }
            OnPropertyChanged(nameof(FeatureReport));
        }
    }
}
