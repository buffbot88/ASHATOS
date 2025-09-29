public ICommand SelectModuleCommand => new RelayCommand<ModuleViewModel>(mod =>
{
    SelectedModule = mod;
});
