using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Application.ReportExport;
using TimeTracker.Application.Reports;

namespace TimeTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel окна отчетов: показывает выборку по проектам, тегам или период
/// и выгружает текущую выборку в файл.
/// Выборка перечитывается при смене вида и при правке периода, поэтому показанные
/// строки всегда соответствуют выбранному отбору; выгружается ровно то, что видно.
/// </summary>
public sealed partial class ReportsViewModel : ObservableObject
{
    private readonly IReportService _reportService;
    private readonly IReportExporter _reportExporter;
    private readonly TimeProvider _timeProvider;

    private Func<Task<string?>> _savePathPicker = null!;
    private IReadOnlyList<DateReportItem> _currentItems = Array.Empty<DateReportItem>();

    /// <summary>
    /// Создает окно отчетов.
    /// </summary>
    /// <param name="reportService">Входящий порт построения отчетов.</param>
    /// <param name="reportExporter">Входящий порт выгрузки отчета в файл.</param>
    /// <param name="timeProvider">Источник времени: от него отсчитывается период по умолчанию.</param>
    public ReportsViewModel(IReportService reportService, IReportExporter reportExporter, TimeProvider timeProvider)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _reportExporter = reportExporter ?? throw new ArgumentNullException(nameof(reportExporter));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        var today = _timeProvider.GetUtcNow().ToLocalTime().Date;

        _from = today.AddDays(-6);
        _to = today.AddDays(1);

        _ = RefreshAsync();
    }

    /// <summary>
    /// Событие запроса закрытия окна.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Строки текущей выборки.
    /// </summary>
    public ObservableCollection<ReportRowViewModel> Rows { get; } = new();

    /// <summary>
    /// Признак того, что выбран отчет по проектам.
    /// </summary>
    [ObservableProperty]
    private bool _isByProject = true;

    /// <summary>
    /// Признак того, что выбран отчет по тегам.
    /// </summary>
    [ObservableProperty]
    private bool _isByTag;

    /// <summary>
    /// Признак того, что выбран отчет по датам.
    /// </summary>
    [ObservableProperty]
    private bool _isByDate;

    /// <summary>
    /// Начало периода; учитывается только для отчета по датам.
    /// </summary>
    [ObservableProperty]
    private DateTime? _from;

    /// <summary>
    /// Конец периода; учитывается только для отчета по датам.
    /// </summary>
    [ObservableProperty]
    private DateTime? _to;

    /// <summary>
    /// Итог по показанным строкам.
    /// </summary>
    [ObservableProperty]
    private string _totalText = string.Empty;

    /// <summary>
    /// Признак того, что выборка пуста.
    /// </summary>
    [ObservableProperty]
    private bool _isEmpty;

    /// <summary>
    /// Признак того, что текущая выборка сохранена в файл.
    /// </summary>
    [ObservableProperty]
    private bool _isSaved;

    /// <summary>
    /// Передает модели диалог выбора файла.
    /// Путь выбирает представление: только окно знает про диалог,
    /// поэтому модель получает готовый путь.
    /// </summary>
    /// <param name="savePathPicker">Функция выбора пути; <c>null</c> — выбор отменен.</param>
    public void AttachSavePathPicker(Func<Task<string?>> savePathPicker)
    {
        _savePathPicker = savePathPicker ?? throw new ArgumentNullException(nameof(savePathPicker));
    }

    /// <summary>
    /// Показывает отчет по проектам.
    /// </summary>
    [RelayCommand]
    private Task ShowByProject() => SwitchAsync(isByProject: true, isByTag: false, isByDate: false);

    /// <summary>
    /// Показывает отчет по тегам.
    /// </summary>
    [RelayCommand]
    private Task ShowByTag() => SwitchAsync(isByProject: false, isByTag: true, isByDate: false);

    /// <summary>
    /// Показывает отчет по датам.
    /// </summary>
    [RelayCommand]
    private Task ShowByDate() => SwitchAsync(isByProject: false, isByTag: false, isByDate: true);

    /// <summary>
    /// Перечитывает выборку по текущему виду.
    /// Период берется из полей окна, а если границы не заданы — подставляется текущий момент,
    /// чтобы запрос всегда был корректным.
    /// </summary>
    public async Task RefreshAsync()
    {
        IsSaved = false;

        Rows.Clear();

        if (IsByProject)
        {
            await LoadByProjectAsync();
        }
        else if (IsByTag)
        {
            await LoadByTagAsync();
        }
        else
        {
            await LoadByDateAsync();
        }

        IsEmpty = Rows.Count == 0;
    }

    /// <summary>
    /// Сохраняет текущую выборку в файл.
    /// </summary>
    [RelayCommand]
    private async Task Export()
    {
        var path = await _savePathPicker();

        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        await _reportExporter.ExportAsync(path, _currentItems);

        IsSaved = true;
    }

    /// <summary>
    /// Закрывает окно отчетов.
    /// </summary>
    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Меняет вид отчета и перечитывает выборку.
    /// </summary>
    /// <param name="isByProject">Признак отчета по проектам.</param>
    /// <param name="isByTag">Признак отчета по тегам.</param>
    /// <param name="isByDate">Признак отчета по датам.</param>
    private async Task SwitchAsync(bool isByProject, bool isByTag, bool isByDate)
    {
        IsByProject = isByProject;
        IsByTag = isByTag;
        IsByDate = isByDate;

        await RefreshAsync();
    }

    /// <summary>
    /// Загружает отчет по проектам.
    /// </summary>
    private async Task LoadByProjectAsync()
    {
        var report = await _reportService.GetByProjectAsync();

        foreach (var item in report.Items)
        {
            Rows.Add(new ReportRowViewModel(item.ProjectName, string.Join(", ", item.TaskNames), item.Total.ToClockString()));
        }

        _currentItems = report.Items.SelectMany(item => item.Items).ToList();
        TotalText = report.Total.ToClockString();
    }

    /// <summary>
    /// Загружает отчет по тегам.
    /// </summary>
    private async Task LoadByTagAsync()
    {
        var report = await _reportService.GetByTagAsync();

        foreach (var item in report.Items)
        {
            Rows.Add(new ReportRowViewModel(item.TagName, string.Join(", ", item.TaskNames), item.Total.ToClockString()));
        }

        _currentItems = report.Items.SelectMany(item => item.Items).ToList();
        TotalText = report.Total.ToClockString();
    }

    /// <summary>
    /// Загружает отчет по датам за выбранный период.
    /// Верхняя граница включает выбранный день целиком, поэтому работа,
    /// начатая сегодня после полуночи, из выборки не выпадает.
    /// </summary>
    private async Task LoadByDateAsync()
    {
        var now = _timeProvider.GetUtcNow();
        var from = From is DateTime start ? new DateTimeOffset(start) : now;
        var to = new DateTimeOffset((To ?? now.LocalDateTime).Date.AddDays(1));

        var report = await _reportService.GetByDateAsync(from, to, projectId: null);

        foreach (var item in report.Items)
        {
            Rows.Add(new ReportRowViewModel(item.TaskName, item.ProjectName, item.Total.ToClockString()));
        }

        _currentItems = report.Items;
        TotalText = report.Total.ToClockString();
    }
}
