using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using RaCore.Engine.Manager;

namespace RaWin.ViewModels
{
    public class FeatureExplorerPanelViewModel : ObservableObject
    {
        private readonly IRaModule? _featureExplorer;

        // Full raw features report string (for selectable/copyable output)
        public string FeatureReport { get; private set; } = "(No report yet)";

        // Optional: parsed list of module names from the report
        public ObservableCollection<string> Modules { get; } = new();

        // Optional: diagnostics/error output (for UI)
        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        // Optional: reload command for UI button
        public ICommand ReloadCommand { get; }

        public FeatureExplorerPanelViewModel(ModuleManager manager)
        {
            _featureExplorer = manager.GetModuleByName("FeatureExplorer");
            ReloadCommand = new RelayCommand(_ => LoadFeatureReport());
            LoadFeatureReport();
        }

        public void LoadFeatureReport()
        {
            try
            {
                Modules.Clear();
                if (_featureExplorer != null)
                {
                    FeatureReport = _featureExplorer.Process("features full") ?? "(No report received)";
                    // Parse module names from the report if format matches previous example
                    // Each module: line "- Name (RaCore.Modules.Name.ModuleType)"
                    var lines = FeatureReport.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var moduleLineRegex = new Regex(@"^\s*-\s*(\w+)\s*\(");
                    foreach (var line in lines)
                    {
                        var match = moduleLineRegex.Match(line);
                        if (match.Success)
                        {
                            Modules.Add(match.Groups[1].Value);
                        }
                    }
                    ErrorMessage = "";
                }
                else
                {
                    FeatureReport = "(FeatureExplorer module not loaded)";
                    ErrorMessage = "FeatureExplorer module not loaded";
                }
            }
            catch (Exception ex)
            {
                FeatureReport = "(Error loading report)";
                ErrorMessage = $"Error: {ex.GetType().Name}: {ex.Message}";
            }
            OnPropertyChanged(nameof(FeatureReport));
            OnPropertyChanged(nameof(Modules));
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }
}
