using CommunityToolkit.Mvvm.ComponentModel;

namespace TimeTracker.Wpf.Mvvm;

/// <summary>
/// База для всех ViewModel приложения.
///
/// Наследуется от ObservableObject (CommunityToolkit.Mvvm) —
/// даёт INotifyPropertyChanged и source-generated свойства через
/// [ObservableProperty]. Наследники помечаются partial,
/// чтобы generator мог добавить реализации.
///
/// Title — общий для всех ViewModel, используется заголовком окна.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Заголовок ViewModel. Может использоваться окном для отображения.
    /// </summary>
    [ObservableProperty]
    private string? _title;
}
