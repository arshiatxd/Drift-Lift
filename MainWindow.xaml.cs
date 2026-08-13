using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DriftLock.ViewModels;
namespace DriftLock
{
    public partial class MainWindow : Window
    {
        private bool _sidebarExpanded = true;
        private const double SidebarExpandedWidth = 200;
        private const double SidebarCollapsedWidth = 64;
        private UIElement? _currentVisibleView;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => {
                ViewHome.IsHitTestVisible = true;
                Panel.SetZIndex(ViewHome, 1);
                FadeInView(ViewHome);
                CheckHidHideOnStartup();
            };
            _currentVisibleView = ViewHome;
        }
        private void CheckHidHideOnStartup()
        {
            try
            {
                if (!DriftLock.Services.HidHideInstallerService.IsHidHideInstalled())
                {
                    var prompt = new DriftLock.Views.Windows.HidHidePromptWindow { Owner = this };
                    prompt.ShowDialog();
                    if (DataContext is DashboardViewModel vm)
                    {
                        vm.CheckHidHide();
                    }
                }
            }
            catch { }
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwndSource = System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            hwndSource?.AddHook(WndProc);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_DEVICECHANGE = 0x0219;
            if (msg == WM_DEVICECHANGE)
            {
                if (DataContext is DashboardViewModel vm)
                {
                    vm.TriggerDeviceRefresh();
                }
            }
            return IntPtr.Zero;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                MaximizeBtn_Click(sender, e);
            else
                DragMove();
        }
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;
        private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            var settings = App.SettingsManager.Settings;
            if (settings.RememberCloseChoice)
            {
                if (settings.MinimizeToTrayOnClose)
                    Hide();
                else
                    Application.Current.Shutdown();
                return;
            }
            var prompt = new DriftLock.Views.Windows.ClosePromptWindow { Owner = this };
            if (prompt.ShowDialog() == true)
            {
                if (prompt.RememberChoice)
                {
                    settings.RememberCloseChoice = true;
                    settings.MinimizeToTrayOnClose = !prompt.ResultExitCompletely;
                    App.SettingsManager.Save();
                }
                if (prompt.ResultExitCompletely)
                    Application.Current.Shutdown();
                else
                    Hide();
            }
        }
        private void TrayIcon_DoubleClick(object sender, RoutedEventArgs e) { Show(); WindowState = WindowState.Normal; Activate(); }
        private void TrayRestore_Click(object sender, RoutedEventArgs e) { Show(); WindowState = WindowState.Normal; Activate(); }
        private void TrayMinimize_Click(object sender, RoutedEventArgs e) => Hide();
        private void TrayExit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        protected override void OnClosing(CancelEventArgs e) { e.Cancel = true; Hide(); base.OnClosing(e); }
        private void SidebarToggle_Click(object sender, RoutedEventArgs e)
        {
            _sidebarExpanded = !_sidebarExpanded;
            double targetWidth = _sidebarExpanded ? SidebarExpandedWidth : SidebarCollapsedWidth;
            AnimateSidebarWidth(targetWidth);
            double labelOpacity = _sidebarExpanded ? 1.0 : 0.0;
            double collapsedOpacity = _sidebarExpanded ? 0.0 : 1.0;
            FadeElement(NavHomeLbl, labelOpacity);
            FadeElement(NavRemapLbl, labelOpacity);
            FadeElement(NavCalibrateLbl, labelOpacity);
            FadeElement(NavMacrosLbl, labelOpacity);
            FadeElement(NavSettingsLbl, labelOpacity);
            FadeElement(DeviceInfoPanel, labelOpacity);
            FadeElement(BatteryInfoPanelExpanded, labelOpacity);
            FadeElement(BatteryInfoPanelCollapsed, collapsedOpacity);
        }
        private void AnimateSidebarWidth(double targetWidth)
        {
            double currentWidth = SidebarBorder.ActualWidth > 0 ? SidebarBorder.ActualWidth : (_sidebarExpanded ? SidebarCollapsedWidth : SidebarExpandedWidth);
            var wa = new DoubleAnimation(currentWidth, targetWidth, new Duration(TimeSpan.FromMilliseconds(240)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            EventHandler frameHandler = (s, e) =>
            {
                SidebarCol.Width = new GridLength(SidebarBorder.ActualWidth);
            };
            CompositionTarget.Rendering += frameHandler;
            wa.Completed += (s, e) =>
            {
                CompositionTarget.Rendering -= frameHandler;
                SidebarCol.Width = new GridLength(targetWidth);
                SidebarBorder.Width = double.NaN;
            };
            SidebarBorder.Width = currentWidth;
            SidebarBorder.BeginAnimation(FrameworkElement.WidthProperty, wa);
        }
        private static void FadeElement(UIElement el, double to)
        {
            var anim = new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(180)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            el.BeginAnimation(OpacityProperty, anim);
        }
        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            UIElement? nextView = null;
            if (NavHome.IsChecked == true)       nextView = ViewHome;
            else if (NavRemap.IsChecked == true)     nextView = ViewRemap;
            else if (NavCalibrate.IsChecked == true)  nextView = ViewCalibrate;
            else if (NavMacros.IsChecked == true)    nextView = ViewMacros;
            else if (NavSettings.IsChecked == true)  nextView = ViewSettings;
            if (nextView == null || nextView == _currentVisibleView) return;
            var prev = _currentVisibleView;
            _currentVisibleView = nextView;
            if (prev != null)
            {
                Panel.SetZIndex(prev, 0);
                prev.IsHitTestVisible = false;
                prev.Opacity = 0;
            }
            Panel.SetZIndex(nextView, 1);
            nextView.IsHitTestVisible = true;
            nextView.Opacity = 1;
            nextView.RenderTransform = null;
        }
        private static void FadeInView(UIElement view)
        {
            view.Opacity = 1;
            view.RenderTransform = null;
        }
    }
}
