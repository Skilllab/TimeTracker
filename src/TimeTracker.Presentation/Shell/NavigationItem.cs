using CommunityToolkit.Mvvm.ComponentModel;
namespace TimeTracker.Presentation.Shell;
/// <summary>
/// Пункт навигации оболочки: заголовок и экран, который он показывает.
/// </summary>
public sealed partial class NavigationItem : ObservableObject
{
    /// <summary>
    /// Создает пункт навигации.
    /// </summary>
    /// <param name="titleKey">Ключ заголовка в словаре строк.</param>
    /// <param name="page">Экран пункта.</param>
    public NavigationItem(string titleKey, object page)
    {
        TitleKey = titleKey;
        Page = page;
    }
    /// <summary>
    /// Ключ заголовка в словаре строк.
    /// </summary>
    public string TitleKey { get; }
    /// <summary>
    /// Заголовок пункта на текущем языке.
    /// </summary>
    public string Title
    {
        get
        {
            if (LocalizationManager.Current != null)
            {
                return LocalizationManager.Current[TitleKey];
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Экран пункта.
    /// </summary>
    public object Page { get; }

    /// <summary>
    /// Обновляет заголовок после смены языка.
    /// </summary>
    public void RefreshTitle()
    {
        OnPropertyChanged(nameof(Title));
    }
}
