using System;
using System.Windows;
using System.Windows.Threading;
using DriftLift.Core;
using DriftLift.Core.Input;
using DriftLift.ViewModels;

namespace DriftLift
{

    public partial class App : Application
    {
        private InputLoop? _inputLoop;
        public static SettingsManager SettingsManager { get; private set; } = null!;

        public static void LogException(Exception? ex, string source)
        {
            if (ex == null) return;
            try
            {
                string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dir = System.IO.Path.Combine(localData, "DriftLift");
                System.IO.Directory.CreateDirectory(dir);
                string path = System.IO.Path.Combine(dir, "crash.log");
                string content = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({source})\n{ex}\n\n";
                System.IO.File.AppendAllText(path, content);
            }
            catch { }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Windows.Media.Animation.Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(System.Windows.Media.Animation.Timeline),
                new FrameworkPropertyMetadata(60));

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            try
            {
                SettingsManager = new SettingsManager();
                _inputLoop = new InputLoop();
                _inputLoop.Start();

                var viewModel = new DashboardViewModel(_inputLoop, SettingsManager);
                var mainWindow = new MainWindow
                {
                    DataContext = viewModel
                };
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                LogException(ex, "OnStartup");
                MessageBox.Show($"Drift-Lift failed to start:\n\n{ex.Message}\n\nCheck crash.log for details.", "Drift-Lift Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _inputLoop?.Stop();
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception, "DispatcherUnhandledException");
            MessageBox.Show($"An unhandled error occurred:\n\n{e.Exception.Message}", "Drift-Lift Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject as Exception, "CurrentDomain_UnhandledException");
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception, "UnobservedTaskException");
            e.SetObserved();
        }
    }
}
