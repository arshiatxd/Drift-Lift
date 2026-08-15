using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DriftLift.Services;

namespace DriftLift.Views.Windows
{
    public partial class ScanGamesDialog : Window
    {
        public ObservableCollection<ScannedGameInfo> DetectedGames { get; } = new();
        public List<ScannedGameInfo> SelectedGames => DetectedGames.Where(g => g.IsSelected).ToList();

        public ScanGamesDialog()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (s, e) => await PerformScanAsync();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private async System.Threading.Tasks.Task PerformScanAsync()
        {
            ScanProgressPanel.Visibility = Visibility.Visible;
            GamesListScroll.Visibility = Visibility.Collapsed;
            NoGamesText.Visibility = Visibility.Collapsed;
            StatusText.Text = "Scanning Steam, EA, Epic Games, and system drives for installed titles...";

            var games = await InstalledGameScannerService.ScanAllInstalledGamesAsync();

            ScanProgressPanel.Visibility = Visibility.Collapsed;
            DetectedGames.Clear();

            if (games.Count > 0)
            {
                foreach (var g in games) DetectedGames.Add(g);
                GamesListScroll.Visibility = Visibility.Visible;
                StatusText.Text = $"Discovered {games.Count} installed games on your PC:";
                UpdateImportButtonText();
            }
            else
            {
                NoGamesText.Visibility = Visibility.Visible;
                StatusText.Text = "No installed game directories detected automatically.";
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateImportButtonText();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var g in DetectedGames) g.IsSelected = true;
            UpdateImportButtonText();
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var g in DetectedGames) g.IsSelected = false;
            UpdateImportButtonText();
        }

        private void UpdateImportButtonText()
        {
            int count = DetectedGames.Count(g => g.IsSelected);
            ImportBtn.Content = count > 0 ? $"IMPORT {count} SELECTED GAME{(count == 1 ? "" : "S")}" : "IMPORT (0)";
            ImportBtn.IsEnabled = count > 0;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
