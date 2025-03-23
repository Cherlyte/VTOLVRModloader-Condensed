using System.Reactive;
using System.Threading.Tasks;
using Mod_Manager.Abstractions;
using Mod_Manager.Models;
using ReactiveUI;

namespace Mod_Manager.ViewModels;

public class PopUpViewModel : ViewModelBase, IPopUpView
{
    private bool _visible;
    public bool Visible
    {
        get => _visible;
        set => this.RaiseAndSetIfChanged(ref _visible, value); 
    }
    private string _title;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value); 
    }
    
    private string _message;
    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value); 
    }
    
    private string _leftButtonText;
    public string LeftButtonText
    {
        get => _leftButtonText;
        set => this.RaiseAndSetIfChanged(ref _leftButtonText, value); 
    }
    
    private string _rightButtonText;
    public string RightButtonText
    {
        get => _rightButtonText;
        set => this.RaiseAndSetIfChanged(ref _rightButtonText, value); 
    }
    
    private bool _leftButtonVisible;
    public bool LeftButtonVisible
    {
        get => _leftButtonVisible;
        set => this.RaiseAndSetIfChanged(ref _leftButtonVisible, value); 
    }
    
    private bool _rightButtonVisible;
    public bool RightButtonVisible
    {
        get => _rightButtonVisible;
        set => this.RaiseAndSetIfChanged(ref _rightButtonVisible, value); 
    }
    public ReactiveCommand<Unit, Unit> LeftButtonCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RightButtonCommand { get; set; }

    private TaskCompletionSource<PopUpButton> _taskCompletionSource;

    public PopUpViewModel()
    {
        LeftButtonCommand = ReactiveCommand.Create(LeftButtonPressed);
        RightButtonCommand = ReactiveCommand.Create(RightButtonPressed);
    }

    public async Task<PopUpButton> ShowPopUp(string title, string message, string? leftButtonText = null, string? rightButtonText = null)
    {
        _taskCompletionSource = new TaskCompletionSource<PopUpButton>();
        Visible = true;
        Title = title;
        Message = message;
        LeftButtonText = leftButtonText ?? string.Empty;
        LeftButtonVisible = leftButtonText != null;
        RightButtonText = rightButtonText ?? string.Empty;
        RightButtonVisible = rightButtonText != null;

        var buttonPressed = await _taskCompletionSource.Task;
        Visible = false;
        return buttonPressed;
    }
    
    public ViewModelBase GetViewModel() => this;

    private void LeftButtonPressed()
    {
        _taskCompletionSource.TrySetResult(PopUpButton.Left);
    }

    private void RightButtonPressed()
    {
        _taskCompletionSource.TrySetResult(PopUpButton.Right);
    }
}