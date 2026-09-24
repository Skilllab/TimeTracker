using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TimeTracker.Presentation.ViewModels;

namespace TimeTracker.Presentation.Views;

/// <summary>
/// Экран списка проектов: показывает список и открывает окно редактора.
/// </summary>
public partial class ProjectsView : UserControl
{
    private ProjectEditorWindow? _editorWindow;

    /// <summary>
    /// Создает экран.
    /// </summary>
    public ProjectsView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Подписывается на состояние редактора при смене модели.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ProjectsViewModel viewModel)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// Открывает и закрывает окно редактора по состоянию модели.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ProjectsViewModel.IsEditorOpen))
        {
            return;
        }

        if (sender is not ProjectsViewModel viewModel)
        {
            return;
        }

        if (viewModel.IsEditorOpen)
        {
            await ShowEditorAsync(viewModel);

            return;
        }

        CloseEditor();
    }

    /// <summary>
    /// Показывает окно редактора.
    /// Окно модальное: пока оно открыто, список проектов недоступен,
    /// поэтому случайная правка соседней строки невозможна.
    /// Повторный вызов при уже открытом окне ничего не делает:
    /// иначе на каждое изменение модели появлялось бы второе окно.
    /// </summary>
    /// <param name="viewModel">Модель экрана проектов.</param>
    private async Task ShowEditorAsync(ProjectsViewModel viewModel)
    {
        if (_editorWindow is not null)
        {
            return;
        }

        var window = new ProjectEditorWindow { DataContext = viewModel };
        window.Closed += OnEditorClosed;

        _editorWindow = window;

        if (TopLevel.GetTopLevel(this) is Window owner)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }
    }

    /// <summary>
    /// Закрывает окно редактора.
    /// Ссылка обнуляется до вызова закрытия: иначе обработчик закрытия
    /// увидел бы ссылку на закрывающееся окно.
    /// </summary>
    private void CloseEditor()
    {
        var window = _editorWindow;

        _editorWindow = null;

        window?.Close();
    }

    /// <summary>
    /// Сбрасывает признак открытого редактора при закрытии окна.
    /// Окно закрывается и крестиком в заголовке, а этот путь не проходит
    /// через команды экрана: без обработки события признак остался бы включенным,
    /// и следующее нажатие кнопки создания не изменило бы значение,
    /// поэтому окно больше не открывалось бы.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnEditorClosed(object? sender, EventArgs e)
    {
        if (sender is ProjectEditorWindow window)
        {
            window.Closed -= OnEditorClosed;
        }

        _editorWindow = null;

        if (DataContext is ProjectsViewModel viewModel)
        {
            viewModel.IsEditorOpen = false;
        }
    }
}
