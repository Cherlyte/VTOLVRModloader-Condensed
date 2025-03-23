using Mod_Manager.Abstractions;
using Mod_Manager.Abstractions.VIewModel;
using ReactiveUI;

namespace Mod_Manager.ViewModels;

internal sealed class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly IViewModel _headerView;

    public ViewModelBase HeaderBarView
    {
        get => _headerView.GetViewModel();
    }

    private IViewModel _mainView;

    public ViewModelBase MainView
    {
        get => _mainView.GetViewModel();
    }
    
    private IViewModel _popUpView;
    public ViewModelBase PopUpView
    {
        get => _popUpView.GetViewModel();
    }

    public MainWindowViewModel(IHeaderBarViewModel header, IHomeViewModel mainView, IPopUpView popUpView)
    {
        _headerView = header;
        _mainView = mainView;
        _popUpView = popUpView;
    }

    public ViewModelBase GetViewModel() => this;
}
