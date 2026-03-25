using System.ComponentModel;
using System.Runtime.CompilerServices;
using patches.Models;

namespace patches.ViewModels;

public sealed class PatchCardViewModel : INotifyPropertyChanged
{
    private string _statusText = "patched";

    public PatchCardViewModel(PatchDefinition definition)
    {
        Definition = definition;
    }

    public PatchDefinition Definition { get; }

    public string Title => Definition.DisplayName;

    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
