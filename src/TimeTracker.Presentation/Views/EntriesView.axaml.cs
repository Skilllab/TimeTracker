using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TimeTracker.Presentation.Shell;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Экран записей 
/// </summary>
public partial class EntriesView : UserControl
{
    /// <summary>
    /// Создание экрана записей
    /// </summary>
    public EntriesView()
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
        if (DataContext is EntriesViewModel viewModel)
        {
            viewModel.AttachNameWindowFactory(CreateNameWindow, this);
        }
    }

    /// <summary>
    /// Создает окно правки имени задачи.
    /// </summary>
    /// <param name="current">Текущее имя задачи.</param>
    private EntryNameWindow CreateNameWindow(string current)
    {
        var title = LocalizationManager.Current?["Entries.EditName"] ?? string.Empty;

        return new EntryNameWindow
        {
            DataContext = new EntryNameViewModel(title, current)
        };
    }
}
