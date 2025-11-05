# RaStudio.py ViewModels (Phase 3)

This folder contains agentic ViewModel classes for RaStudio.py’s modular, plugin-ready UI.
ViewModels orchestrate UI state, commands, and agentic backend flows via SpeechPipelineService and ModuleBus.

## Structure

- **ObservableObject**: Base for property notification and MVVM reactivity.
- **RelayCommand**: Command pattern for UI actions (sync/async).
- **Panel ViewModels**: Dashboard, Commands, Settings, Diagnostics, Metrics, Logs, etc.
- **Page ViewModels**: Modular orchestration for tabs/pages.
- **MainWindowViewModel**: Root window VM, orchestrates diagnostics/events, theme/language, module reloads.
- **MonitorTabViewModel**: Legacy/compat tab for monitoring.

## Wiring Guide

- Use your Python UI toolkit to bind UI elements to ViewModel properties and commands.
- All agentic input/output flows through SpeechPipelineService.
- Diagnostics/events are published via ModuleBus for real-time UI updates.

## Extending

- Add new panels/pages by extending the ViewModel base classes.
- Support plugin/extension ViewModels by wiring through SpeechPipelineService and ModuleBus.

---

**Ready for full integration with RaCore v2.5, plugin/module wiring, and advanced agentic flows!**