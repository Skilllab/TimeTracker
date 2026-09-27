using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application;
using TimeTracker.Domain;
using TimeTracker.Presentation.Shell;
using TimeTracker.Presentation.Views;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel списка задач: показывает плашки задач, отбирает их минипоиском по имени,
/// минипоиском по тегам и выбранным проектом и управляет задачами через входящие порты.
/// Плашки обновляются на месте, поэтому список не пересобирается и не мигает,
/// пока время выполняемой задачи растет.
/// </summary>
public sealed partial class TasksViewModel : ObservableObject
{
    private readonly ITaskList _taskList;
    private readonly ITaskControl _taskControl;
    private readonly IProjectList _projectList;
    private readonly ITaskTagSuggestions _tagSuggestions;
    private readonly LocalizationManager _localizationManager;
    private readonly DispatcherTimer _ticker = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly Dictionary<Guid, TaskCardViewModel> _cards = new();
    private Func<string, string, EntryNameWindow> _nameWindowFactory = null!;
    private Func<string, IReadOnlyList<Project>, Guid?, Guid, ProjectPickerWindow> _projectWindowFactory = null!;
    private Func<string, string, Guid, IReadOnlyList<string>, TaskTagsWindow> _tagsWindowFactory = null!;
    private Func<string, string, ConfirmWindow> _confirmWindowFactory = null!;
    private Control _owner = null!;
    private List<Guid> _order = new();
    private bool _updatingFilters;

    /// <summary>
    /// Создает экран списка задач.
    /// </summary>
    /// <param name="taskList">Входящий порт списка задач.</param>
    /// <param name="taskControl">Входящий порт управления задачами.</param>
    /// <param name="projectList">Входящий порт списка проектов.</param>
    /// <param name="localizationManager">Управление языком.</param>
    /// <param name="tagSuggestions">Входящий порт подсказки имен тегов.</param>
    public TasksViewModel(
        ITaskList taskList,
        ITaskControl taskControl,
        IProjectList projectList,
        LocalizationManager localizationManager,
        ITaskTagSuggestions tagSuggestions)
    {
        _taskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
        _taskControl = taskControl ?? throw new ArgumentNullException(nameof(taskControl));
        _projectList = projectList ?? throw new ArgumentNullException(nameof(projectList));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _tagSuggestions = tagSuggestions ?? throw new ArgumentNullException(nameof(tagSuggestions));

        _localizationManager.Changed += OnLanguageChanged;

        _ticker.Tick += OnTick;
        _ticker.Start();

        _ = RefreshFiltersAsync();
    }

    /// <summary>
    /// Плашки задач в порядке показа.
    /// </summary>
    public ObservableCollection<TaskCardViewModel> Cards { get; } = new();

    /// <summary>
    /// Пункты отбора по проекту: первым идет «Все проекты».
    /// </summary>
    public ObservableCollection<ProjectFilterItem> ProjectFilters { get; } = new();

    /// <summary>
    /// Строка минипоиска: фильтрует список по наименованию и не меняет состояние задач.
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Строка минипоиска по тегам: фильтрует список по именам тегов задачи.
    /// </summary>
    [ObservableProperty]
    private string _tagSearchText = string.Empty;

    /// <summary>
    /// Признак того, что в списке показываются и удаленные задачи.
    /// По умолчанию удаленные скрыты, поэтому список показывает только действующие задачи.
    /// </summary>
    [ObservableProperty]
    private bool _showDeleted;

    /// <summary>
    /// Выбранный отбор по проекту; пункт «Все проекты» означает отсутствие отбора.
    /// </summary>
    [ObservableProperty]
    private ProjectFilterItem? _selectedProjectFilter;

    /// <summary>
    /// Признак того, что список задач пуст.
    /// </summary>
    public bool IsEmpty => _order.Count == 0;

    /// <summary>
    /// Создает модель окна выбора проекта задачи.
    /// Модель создает сам экран: он владеет входящими портами, а окно показывает готовую модель.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="projects">Проекты, доступные для выбора.</param>
    /// <param name="selectedProjectId">Идентификатор текущего проекта; <c>null</c>, если проекта нет.</param>
    /// <param name="taskId">Идентификатор задачи, у которой меняется проект.</param>
    public ProjectPickerViewModel CreateProjectPicker(
        string title,
        IReadOnlyList<Project> projects,
        Guid? selectedProjectId,
        Guid taskId)
        => new(title, projects, selectedProjectId, () => CanChangeProjectAsync(taskId), _localizationManager["Tasks.ProjectBusy"]);

    /// <summary>
    /// Создает модель окна тегов задачи.
    /// Модель создает сам экран: он владеет входящими портами, а окно показывает готовую модель.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="taskTitle">Наименование задачи.</param>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="assignedTags">Теги, назначенные задаче.</param>
    public TaskTagsViewModel CreateTaskTags(
        string title,
        string taskTitle,
        Guid taskId,
        IReadOnlyList<string> assignedTags)
        => new(title, taskTitle, taskId, assignedTags, _taskControl, _tagSuggestions);

    /// <summary>
    /// Передает модели фабрики окон и владельца окна.
    /// Окна создает код разметки: показ окна — работа представления,
    /// а владелец берется из визуального дерева, поэтому окна открываются модально.
    /// </summary>
    /// <param name="nameWindowFactory">Фабрика окна имени задачи: заголовок и текущее имя.</param>
    /// <param name="projectWindowFactory">Фабрика окна выбора проекта задачи.</param>
    /// <param name="tagsWindowFactory">Фабрика окна тегов задачи.</param>
    /// <param name="confirmWindowFactory">Фабрика окна подтверждения.</param>
    /// <param name="owner">Элемент управления, по которому ищется владелец окна.</param>
    public void AttachWindowFactories(
        Func<string, string, EntryNameWindow> nameWindowFactory,
        Func<string, IReadOnlyList<Project>, Guid?, Guid, ProjectPickerWindow> projectWindowFactory,
        Func<string, string, Guid, IReadOnlyList<string>, TaskTagsWindow> tagsWindowFactory,
        Func<string, string, ConfirmWindow> confirmWindowFactory,
        Control owner)
    {
        _nameWindowFactory = nameWindowFactory ?? throw new ArgumentNullException(nameof(nameWindowFactory));
        _projectWindowFactory = projectWindowFactory ?? throw new ArgumentNullException(nameof(projectWindowFactory));
        _tagsWindowFactory = tagsWindowFactory ?? throw new ArgumentNullException(nameof(tagsWindowFactory));
        _confirmWindowFactory = confirmWindowFactory ?? throw new ArgumentNullException(nameof(confirmWindowFactory));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    /// <summary>
    /// Перечитывает пункты отбора по проекту и список задач.
    /// Первым пунктом идет «Все проекты».
    /// Прежний отбор сохраняется: если проект исчез из списка, отбор возвращается к «Все проекты».
    /// </summary>
    public async Task RefreshFiltersAsync()
    {
        var selectedId = SelectedProjectFilter?.Id;
        var projects = await _projectList.GetAvailableAsync();

        _updatingFilters = true;

        try
        {
            ProjectFilters.Clear();
            ProjectFilters.Add(new ProjectFilterItem(null, _localizationManager["Tasks.AllProjects"], string.Empty));

            foreach (var project in projects)
            {
                ProjectFilters.Add(new ProjectFilterItem(project.Id, project.Name, project.Color));
            }

            SelectedProjectFilter = ProjectFilters.FirstOrDefault(item => item.Id == selectedId) ?? ProjectFilters[0];
        }
        finally
        {
            _updatingFilters = false;
        }

        await RefreshAsync();
    }

    /// <summary>
    /// Перечитывает задачи и обновляет плашки.
    /// Список пересобирается только при смене состава или порядка:
    /// иначе обновляются уже показанные плашки, поэтому список не мигает.
    /// Отбор идет минипоиском по имени, минипоиском по тегам и выбранным проектом.
    /// </summary>
    public async Task RefreshAsync()
    {
        var items = await _taskList.GetAsync(SearchText, TagSearchText, SelectedProjectFilter?.Id, includeDeleted: ShowDeleted);
        var cards = new List<TaskCardViewModel>(items.Count);

        foreach (var item in items)
        {
            if (!_cards.TryGetValue(item.Id, out var card))
            {
                card = new TaskCardViewModel(item);

                _cards[item.Id] = card;
            }

            card.Update(item);

            cards.Add(card);
        }

        var order = cards.Select(card => card.Id).ToList();

        if (!order.SequenceEqual(_order) || Cards.Count == 0)
        {
            _order = order;

            Cards.Clear();

            foreach (var card in cards)
            {
                Cards.Add(card);
            }

            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    /// <summary>
    /// Открывает окно имени и создает задачу с введенным именем.
    /// Задача создается неначатой: отсчет начнется при запуске,
    /// а проект задается позже нажатием по имени проекта на плашке.
    /// </summary>
    [RelayCommand]
    private async Task CreateTask()
    {
        var name = await RequestNameAsync(_localizationManager["Tasks.New"], string.Empty);

        if (name is null)
        {
            return;
        }

        await _taskControl.CreateTaskAsync(name, null);

        await RefreshAsync();
    }

    /// <summary>
    /// Переключает состояние задачи: запускает, приостанавливает или продолжает.
    /// Действие выбирается по текущему состоянию, поэтому одной кнопки достаточно.
    /// </summary>
    /// <param name="card">Плашка задачи.</param>
    [RelayCommand]
    private async Task ToggleTask(TaskCardViewModel? card)
    {
        if (card is null || card.IsDeleted)
        {
            return;
        }

        if (card.IsRunning)
        {
            await _taskControl.PauseAsync(card.Id);
        }
        else if (card.IsPaused)
        {
            await _taskControl.ResumeAsync(card.Id);
        }
        else
        {
            await _taskControl.StartTaskAsync(card.Id);
        }

        await RefreshAsync();
    }

    /// <summary>
    /// Открывает окно выбора проекта задачи.
    /// У выполняемой задачи окно не открывается: проект меняют только у задачи, которая не идет.
    /// </summary>
    /// <param name="card">Плашка задачи.</param>
    [RelayCommand]
    private async Task ChangeProjectTask(TaskCardViewModel? card)
    {
        if (card is null || card.IsDeleted || card.IsRunning)
        {
            return;
        }

        var projects = await _projectList.GetAvailableAsync();
        var title = _localizationManager["Tasks.Project"];
        var window = _projectWindowFactory(title, projects, card.ProjectId, card.Id);

        if (TopLevel.GetTopLevel(_owner) is Window owner)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }

        if (window.DataContext is ProjectPickerViewModel model && model.IsConfirmed)
        {
            await _taskControl.ChangeProjectAsync(card.Id, model.SelectedProjectId);
        }

        await RefreshFiltersAsync();
    }

    /// <summary>
    /// Открывает окно тегов задачи.
    /// Окно модальное, поэтому одновременно открыто не более одного окна тегов;
    /// после закрытия список перечитывается: теги участвуют в минипоиске по тегам.
    /// </summary>
    /// <param name="card">Плашка задачи.</param>
    [RelayCommand]
    private async Task OpenTaskTags(TaskCardViewModel? card)
    {
        if (card is null || card.IsDeleted)
        {
            return;
        }

        var title = _localizationManager["Tasks.EditTags"];
        var window = _tagsWindowFactory(title, card.Title, card.Id, card.TagNames);

        if (TopLevel.GetTopLevel(_owner) is Window owner)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }

        if (window.DataContext is TaskTagsViewModel model && model.IsChanged)
        {
            await RefreshAsync();
        }
    }

    /// <summary>
    /// Проверяет, можно ли сменить проект задачи.
    /// Состояние читается заново: пока окно открыто, задача могла начаться или быть удалена.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    private async Task<bool> CanChangeProjectAsync(Guid taskId)
    {
        var items = await _taskList.GetAsync(null, null, null, includeDeleted: ShowDeleted);
        var item = items.FirstOrDefault(candidate => candidate.Id == taskId);

        return item is not null && !item.IsRunning && !item.IsDeleted;
    }

    /// <summary>
    /// Завершает задачу.
    /// </summary>
    /// <param name="card">Плашка задачи.</param>
    [RelayCommand]
    private async Task FinishTask(TaskCardViewModel? card)
    {
        if (card is null || card.IsDeleted)
        {
            return;
        }

        await _taskControl.FinishAsync(card.Id);

        await RefreshAsync();
    }

    /// <summary>
    /// Возвращает завершенную задачу в работу.
    /// </summary>
    /// <param name="card">Плашка задачи.</param>
    [RelayCommand]
    private async Task ReopenTask(TaskCardViewModel? card)
    {
        if (card is null || card.IsDeleted)
        {
            return;
        }

        await _taskControl.ReopenAsync(card.Id);

        await RefreshAsync();
    }

    /// <summary>
    /// Переименовывает задачу через окно ввода имени.
    /// Удаленную задачу не переименовывают: она ждет восстановления или полного удаления.
    /// Идущую задачу переименовывать можно: имя не влияет на отсчет времени.
    /// Порт вызывается только когда имя подтверждено и отличается от текущего.
    /// </summary>
    /// <param name="card">Плашка задачи.</param>
    [RelayCommand]
    private async Task RenameTask(TaskCardViewModel? card)
    {
        if (card is null || card.IsDeleted)
        {
            return;
        }

        var name = await RequestNameAsync(_localizationManager["Tasks.Rename"], card.Title);

        if (name is null || string.Equals(name, card.Title, StringComparison.Ordinal))
        {
            return;
        }

        await _taskControl.RenameAsync(card.Id, name);

        await RefreshAsync();
    }

    /// <summary>
    /// Перечитывает список при включении или выключении показа удаленных задач.
    /// </summary>
    /// <param name="value">Признак того, что удаленные задачи показываются.</param>
    partial void OnShowDeletedChanged(bool value) => _ = RefreshAsync();

    /// <summary>
    /// Помечает задачу удаленной.
    /// Идущую задачу удалить нельзя: кнопка у нее не показывается.
    /// </summary>
    /// <param name="card">Плашка задачи.</param>
    [RelayCommand]
    private async Task DeleteTask(TaskCardViewModel? card)
    {
        if (card is null)
        {
            return;
        }

        await _taskControl.DeleteTaskAsync(card.Id);

        await RefreshAsync();
    }

    /// <summary>
    /// Снимает с задачи пометку удаления.
    /// </summary>
    /// <param name="card">Плашка задачи.</param>
    [RelayCommand]
    private async Task RestoreTask(TaskCardViewModel? card)
    {
        if (card is null)
        {
            return;
        }

        await _taskControl.RestoreTaskAsync(card.Id);

        await RefreshAsync();
    }

    /// <summary>
    /// Удаляет задачу из хранилища вместе с ее записями времени.
    /// Действие необратимо, поэтому перед ним запрашивается подтверждение.
    /// </summary>
    /// <param name="card">Плашка задачи.</param>
    [RelayCommand]
    private async Task DeletePermanently(TaskCardViewModel? card)
    {
        if (card is null)
        {
            return;
        }

        var title = _localizationManager["Tasks.DeletePermanentlyTitle"];
        var question = _localizationManager["Tasks.DeletePermanentlyQuestion"];
        var window = _confirmWindowFactory(title, question);

        if (TopLevel.GetTopLevel(_owner) is Window owner)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }

        if (window.DataContext is not ConfirmViewModel model || !model.IsConfirmed)
        {
            return;
        }

        await _taskControl.DeletePermanentlyAsync(card.Id);

        await RefreshAsync();
    }

    /// <summary>
    /// Запрашивает имя задачи в отдельном окне; <c>null</c>, если пользователь отказался.
    /// Заголовок передается вызывающим: при создании и при переименовании он разный.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="current">Текущее имя задачи.</param>
    private async Task<string?> RequestNameAsync(string title, string current)
    {
        var window = _nameWindowFactory(title, current);

        if (TopLevel.GetTopLevel(_owner) is Window owner)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }

        return window.DataContext is EntryNameViewModel model && model.IsConfirmed
            ? model.TrimmedName
            : null;
    }

    /// <summary>
    /// Перечитывает список раз в секунду, пока идет хотя бы одна задача:
    /// так время выполняемой задачи растет на экране, а в покое база данных не читается.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnTick(object? sender, EventArgs e)
    {
        if (Cards.Any(card => card.IsRunning))
        {
            _ = RefreshAsync();
        }
    }

    /// <summary>
    /// Перечитывает список при смене отбора по проекту.
    /// </summary>
    /// <param name="value">Выбранный отбор; «Все проекты» означает отсутствие отбора.</param>
    partial void OnSelectedProjectFilterChanged(ProjectFilterItem? value)
    {
        if (_updatingFilters)
        {
            return;
        }

        _ = RefreshAsync();
    }

    /// <summary>
    /// Перечитывает список при изменении строки минипоиска.
    /// Отбор по проекту сбрасывается на «Все проекты»: иначе поиск шел бы только внутри выбранного проекта.
    /// </summary>
    /// <param name="value">Новый текст минипоиска.</param>
    partial void OnSearchTextChanged(string value)
    {
        ResetProjectFilter();

        _ = RefreshAsync();
    }

    /// <summary>
    /// Перечитывает список при изменении строки минипоиска по тегам.
    /// Отбор по проекту сбрасывается на «Все проекты»: поиск по тегам идет по всем проектам.
    /// </summary>
    /// <param name="value">Новый текст минипоиска по тегам.</param>
    partial void OnTagSearchTextChanged(string value)
    {
        ResetProjectFilter();

        _ = RefreshAsync();
    }

    /// <summary>
    /// Возвращает отбор по проекту к «Все проекты».
    /// Значение ставится с подавлением обработчика: обновление списка запускает сам вызывающий,
    /// иначе чтение выполнялось бы дважды.
    /// </summary>
    private void ResetProjectFilter()
    {
        if (SelectedProjectFilter?.Id is null || ProjectFilters.Count == 0)
        {
            return;
        }

        _updatingFilters = true;

        try
        {
            SelectedProjectFilter = ProjectFilters[0];
        }
        finally
        {
            _updatingFilters = false;
        }
    }

    /// <summary>
    /// Перечитывает отбор и список после смены языка: подписи пунктов берутся из словаря.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        _order = new List<Guid>();

        _ = RefreshFiltersAsync();
    }
}
