using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Wpf.Navigation;

namespace TimeTracker.Wpf.Mvvm.Behaviors;

/// <summary>
/// Attached behavior: автоматически устанавливает DataContext
/// для View из DI.
///
/// Использование в XAML:
///   &lt;Window ... local:ViewModelAutoWireBehavior.AutoWire="True"&gt;
///
/// При установке AutoWire=True behavior:
///   1. Смотрит на тип View.
///   2. Находит соответствующий ViewModel через ViewMapping.
///   3. Резолвит ViewModel из DI.
///   4. Устанавливает как DataContext.
///
/// В Design Mode (VS/Rider designer) ничего не делает —
/// иначе IDE падает без настроенного DI.
/// </summary>
public static class ViewModelAutoWireBehavior
{
    public static readonly DependencyProperty AutoWireProperty =
        DependencyProperty.RegisterAttached(
            "AutoWire",
            typeof(bool),
            typeof(ViewModelAutoWireBehavior),
            new PropertyMetadata(false, OnAutoWireChanged));

    public static bool GetAutoWire(DependencyObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return (bool)obj.GetValue(AutoWireProperty);
    }

    public static void SetAutoWire(DependencyObject obj, bool value)
    {
        ArgumentNullException.ThrowIfNull(obj);
        obj.SetValue(AutoWireProperty, value);
    }

    private static void OnAutoWireChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement view || !(bool)e.NewValue)
            return;

        // В design mode не резолвим DataContext — нет работающего DI.
        if (DesignerProperties.GetIsInDesignMode(view))
            return;

        var viewModelType = ViewMapping.GetViewModelType(view.GetType());
        if (viewModelType is null)
            return;

        // App.Current — наш App с Host и ServiceProvider.
        if (System.Windows.Application.Current is not App app || app.Services is null)
            return;

        view.DataContext = app.Services.GetRequiredService(viewModelType);
    }
}
