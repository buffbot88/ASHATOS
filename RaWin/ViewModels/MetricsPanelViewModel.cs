using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RaCore.Engine.Manager;

namespace RaWin.ViewModels
{
    /// <summary>
    /// ViewModel for the Metrics card in Monitoring.
    /// </summary>
    public class MetricsPanelViewModel : INotifyPropertyChanged
    {
        private double _cpuUsage;
        public double CpuUsage
        {
            get => _cpuUsage;
            set { _cpuUsage = value; OnPropertyChanged(); }
        }

        private double _ramUsage;
        public double RamUsage
        {
            get => _ramUsage;
            set { _ramUsage = value; OnPropertyChanged(); }
        }

        private int _threadCount;
        public int ThreadCount
        {
            get => _threadCount;
            set { _threadCount = value; OnPropertyChanged(); }
        }

        private int _activeModuleCount;
        public int ActiveModuleCount
        {
            get => _activeModuleCount;
            set { _activeModuleCount = value; OnPropertyChanged(); }
        }

        public MetricsPanelViewModel(ModuleManager manager)
        {
            // Try to get metrics from module first
            var core = manager.CoreModules.FirstOrDefault();
            bool gotMetrics = false;

            if (core?.Instance != null)
            {
                double.TryParse(core.Instance.Process("cpu"), out var cpu);
                double.TryParse(core.Instance.Process("ram"), out var ram);
                int.TryParse(core.Instance.Process("threads"), out var threads);

                // If any metric is nonzero, count as "got metrics"
                if (cpu > 0 || ram > 0 || threads > 0)
                {
                    CpuUsage = cpu;
                    RamUsage = ram;
                    ThreadCount = threads;
                    ActiveModuleCount = manager.CoreModules.Count;
                    gotMetrics = true;
                }
            }

            // If metrics not available from module, use system metrics as fallback
            if (!gotMetrics)
            {
                CpuUsage = GetProcessCpuUsage();
                RamUsage = GetProcessRamUsageMB();
                ThreadCount = Process.GetCurrentProcess().Threads.Count;
                ActiveModuleCount = manager.CoreModules.Count;
            }
        }

        // Returns 0 for now – you can implement actual CPU usage logic if desired (see below)
        private double GetProcessCpuUsage()
        {
            // For real CPU usage, use PerformanceCounter or Process APIs, but this is just a stub:
            return 0;
        }

        private double GetProcessRamUsageMB()
        {
            return Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
