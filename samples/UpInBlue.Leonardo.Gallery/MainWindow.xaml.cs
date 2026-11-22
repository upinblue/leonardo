using System.ComponentModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using UpInBlue.Leonardo.Gallery.Services;
using UpInBlue.Leonardo.WinUI.Motion;
using Windows.Graphics;

namespace UpInBlue.Leonardo.Gallery;

/// <summary>
/// The main window: title bar, navigation and the page frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>
    /// Initializes the main window and opens the first page.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = 500;
            presenter.PreferredMinimumHeight = 500;
        }

        var options = StartupOptions.Current;
        if (options.Width is { } width && options.Height is { } height)
        {
            AppWindow.Resize(new SizeInt32(width, height));
        }

        App.SystemSettings = new SystemState(AppWindow.Id);

        // Leonardo motion: listen to the Windows setting, and let any click or key press end running motion.
        MotionSettings.Initialize();
        MotionSettings.AnimationsOverride = options.Animations;
        MotionInterruption.Attach(Root);
        ApplyTheme();
        App.Settings.PropertyChanged += OnSettingsChanged;
        Closed += (_, _) => App.Settings.PropertyChanged -= OnSettingsChanged;

        Navigation.Loaded += (_, _) => SyncSelection();
        Navigate(GalleryPages.Find(options.Page) ?? GalleryPages.All[0]);
    }

    private void Navigate(GalleryPage page)
    {
        if (ContentFrame.CurrentSourcePageType != page.PageType)
        {
            ContentFrame.Navigate(page.PageType, null, new SuppressNavigationTransitionInfo());
        }
    }

    private void OnNavigationItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var tag = args.IsSettingsInvoked ? GalleryPages.SettingsTag : args.InvokedItemContainer?.Tag as string;
        if (GalleryPages.Find(tag) is { } page)
        {
            Navigate(page);
        }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        AppTitleBar.IsBackButtonEnabled = ContentFrame.CanGoBack;
        SyncSelection();
    }

    /// <summary>
    /// Selects the navigation item of the current page. The settings item exists only once the navigation is loaded.
    /// </summary>
    private void SyncSelection()
    {
        var page = GalleryPages.Find(ContentFrame.CurrentSourcePageType);
        if (page?.Tag == GalleryPages.SettingsTag)
        {
            Navigation.SelectedItem = Navigation.SettingsItem;
            return;
        }

        Navigation.SelectedItem = Navigation.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(item => item.Tag as string == page?.Tag);
    }

    private void OnBackRequested(TitleBar sender, object args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }

    private void OnPaneToggleRequested(TitleBar sender, object args) => Navigation.IsPaneOpen = !Navigation.IsPaneOpen;

    /// <summary>
    /// F6 and Shift+F6 move focus between the two areas of the window: navigation and page content.
    /// </summary>
    private void OnF6Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        var focused = FocusManager.GetFocusedElement(Root.XamlRoot) as DependencyObject;
        if (IsInside(focused, ContentFrame))
        {
            if (Navigation.SelectedItem is Control selected)
            {
                selected.Focus(FocusState.Keyboard);
            }

            return;
        }

        if (FocusManager.FindFirstFocusableElement(ContentFrame) is Control first)
        {
            first.Focus(FocusState.Keyboard);
        }
    }

    private static bool IsInside(DependencyObject? element, DependencyObject container)
    {
        for (var current = element; current is not null; current = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(current))
        {
            if (current == container)
            {
                return true;
            }
        }

        return false;
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppSettings.Theme))
        {
            ApplyTheme();
        }
    }

    private void ApplyTheme()
    {
        var theme = App.Settings.Theme;
        Root.RequestedTheme = theme;
        AppWindow.TitleBar.PreferredTheme = theme switch
        {
            ElementTheme.Light => TitleBarTheme.Light,
            ElementTheme.Dark => TitleBarTheme.Dark,
            _ => TitleBarTheme.UseDefaultAppMode,
        };
    }
}
