using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Windows.UI;
using WinRT;

namespace LDS.UI.Services;

public interface IBackDropController
{
    void SetAcrylicBackdrop(Window window);
}

public class BackDropController : IBackDropController
{
    private readonly DesktopAcrylicController m_backdropController = new() { TintColor = Color.FromArgb(0, 0, 0, 0), TintOpacity = 0, LuminosityOpacity = 0 };
    private readonly SystemBackdropConfiguration m_configurationSource = new() { IsInputActive = true, Theme = SystemBackdropTheme.Dark };

    private Window? m_window;

    public void SetAcrylicBackdrop(Window window) {
        if (!DesktopAcrylicController.IsSupported()) {
            return;
        }

        m_window = window;

        m_window.Activated += Window_Activated;
        m_window.Closed += Window_Closed;
        ((FrameworkElement)m_window.Content).ActualThemeChanged += Window_ThemeChanged;

        m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);
        m_backdropController.AddSystemBackdropTarget(m_window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args) => m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;

    private void Window_Closed(object sender, WindowEventArgs args) {
        m_backdropController?.Dispose();
        if (m_window != null) {
            m_window.Activated -= Window_Activated;
            m_window.Closed -= Window_Closed;
            ((FrameworkElement)m_window.Content).ActualThemeChanged -= Window_ThemeChanged;
            m_window = null;

        }
    }

    private void Window_ThemeChanged(FrameworkElement sender, object args) {
        if (m_configurationSource != null) {
            SetConfigurationSourceTheme();
        }
    }

    private void SetConfigurationSourceTheme() {
        m_configurationSource.Theme = ((FrameworkElement)m_window!.Content).ActualTheme switch {
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            ElementTheme.Light => SystemBackdropTheme.Light,
            ElementTheme.Default => SystemBackdropTheme.Default,
            _ => SystemBackdropTheme.Default
        };
    }
}