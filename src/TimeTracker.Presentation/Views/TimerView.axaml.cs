using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TimeTracker.Presentation.Shell;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Таймер для отслеживания времени.
/// </summary>
public partial class TimerView : UserControl
{
    /// <summary>
    /// Создает экран таймера.
    /// </summary>
    public TimerView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Передает модели фабрику окна имени и владельца окна.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is TimerViewModel viewModel)
        {
            viewModel.AttachNameWindowFactory(CreateNameWindow, this);
        }
    }

    /// <summary>
    /// Создает окно ввода имени задачи.
    /// </summary>
    /// <param name="current">Текущее имя задачи.</param>
    private EntryNameWindow CreateNameWindow(string current)
    {
        var title = LocalizationManager.Current?["Entries.NewName"] ?? string.Empty;

        return new EntryNameWindow
        {
            DataContext = new EntryNameViewModel(title, current)
        };
    }
}
