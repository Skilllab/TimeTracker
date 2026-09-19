using System.Windows;

namespace TimeTracker.Wpf.Navigation;

/// <summary>
/// Реестр соответствий View ↔ ViewModel.
///
/// Регистрация — при старте приложения в AddWpf().
/// ViewModelAutoWireBehavior и NavigationService используют
/// этот реестр, чтобы резолвить ViewModel для View и наоборот.
///
/// Хранится как статический словарь — единая точка правды.
/// Регистрация идемпотентна: повторный Register перезаписывает.
/// </summary>
public static class ViewMapping
{
    private static readonly Dictionary<Type, Type> ViewToViewModel = [];
    private static readonly Dictionary<Type, Type> ViewModelToView = [];

    public static void Register<TView, TViewModel>()
        where TView : FrameworkElement
        where TViewModel : Mvvm.ViewModelBase
    {
        ViewToViewModel[typeof(TView)] = typeof(TViewModel);
        ViewModelToView[typeof(TViewModel)] = typeof(TView);
    }

    public static Type? GetViewModelType(Type viewType) =>
        ViewToViewModel.TryGetValue(viewType, out var vm) ? vm : null;

    public static Type? GetViewType(Type viewModelType) =>
        ViewModelToView.TryGetValue(viewModelType, out var view) ? view : null;
}
