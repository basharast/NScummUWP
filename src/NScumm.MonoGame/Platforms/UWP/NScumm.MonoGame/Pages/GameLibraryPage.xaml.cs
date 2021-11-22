using System.Diagnostics;
using NScumm.MonoGame.Converters;
using NScumm.MonoGame.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.ApplicationModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;
using System.Runtime.InteropServices;
using Windows.UI.Xaml.Controls;

namespace NScumm.MonoGame
{
    public sealed partial class GameLibraryPage
    {
        public IGameLibraryViewModel ViewModel
        {
            get { return DataContext as IGameLibraryViewModel; }
            set { DataContext = value; }
        }

        IProgress<double> scanningProgress;

        public GameLibraryPage()
        {
            InitializeComponent();
            try
            {
                var currentView = SystemNavigationManager.GetForCurrentView();
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            catch (Exception e)
            {
            }


            DataContext = new GameLibraryViewModel();
            NoGameTextBlock.SetBinding(VisibilityProperty, new Binding { Path = new PropertyPath("ShowNoGameMessage"), Converter = new ShowNoGameMessageToVisibilityConverter() });
            ProgressPanel.SetBinding(VisibilityProperty, new Binding { Path = new PropertyPath("IsScanning"), Converter = new IsScanningToVisibilityConverter() });
            ScanningGamesTextBlockProgress.SetBinding(ProgressBar.ValueProperty, new Binding { Path = new PropertyPath("LoadingProgressValue") });
            
            GamePage.ShowTile(WidePreviewTile, 1500, "Welcome", $"NSCUMM v{GetAppVersion()}");
        }

        public static string GetAppVersion()
        {
            try
            {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
            catch (Exception ex)
            {

            }
            return "1.0.0.0";
        }
        private async void OnLaunchGame(object sender, Windows.UI.Xaml.Controls.ItemClickEventArgs e)
        {
            // navigate to the game
            var vm = e.ClickedItem as GameViewModel;
            if (vm != null)
            {
                GamePage.ShowTile(WidePreviewTile, 1500, "Starting Game", $"{vm.Game.Game.Description}", new string[] { $"Id: {vm.Game.Game.Id}", $"Platform: {vm.Game.Game.Platform}", $"Pixel: {vm.Game.Game.PixelFormat}" });
                await Task.Delay(1500);
                Frame.Navigate(typeof(GamePage), vm.Game);
            }
        }

        private async void ShowDialog(string message)
        {
            var messageDialog = new MessageDialog(message);
            messageDialog.Commands.Add(new UICommand(
                "Close"));
            await messageDialog.ShowAsync();
        }
        private async Task ShowNotice(string message)
        {
            var messageDialog = new MessageDialog(message);
            messageDialog.Commands.Add(new UICommand(
                "OK"));
            await messageDialog.ShowAsync();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog("1-Extract game content to folder\n2-Click add and select the folder\n3-Enjoy\n\nShortcuts\nF5: Show Menu\nSpace: Pause\nEsc: Skip\n\nCreated by scemino\nEnhanced by Bashar Astifan\nGitHub: https://github.com/basharast/NScummUWP");
        }

        private async void ImportDataBase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ShowNotice("Remember, it will replace your previous database (if imported before)\nDon't worry, It will not affect on the built-in database");
                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                filePicker.FileTypeFilter.Add(".dat");
                var documentFile = await filePicker.PickSingleFileAsync();
                if (documentFile != null)
                {
                    TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await documentFile.CopyAsync(ApplicationData.Current.LocalFolder, "ScummVM.dat", NameCollisionOption.ReplaceExisting);
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                            {
                                try
                                {
                                    GamePage.ShowTile(WidePreviewTile, 2500, "Import Database", "Database imported", new string[] { "It will be used", "Beside the built-in" });
                                }
                                catch (Exception ex)
                                {

                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            ShowDialog(ex);
                        }
                        try
                        {
                            taskCompletionSource.SetResult(true);
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                    await taskCompletionSource.Task;
                    //ShowDialog("Import Done");
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }
        private async void ShowDialog(Exception message)
        {
            var messageDialog = new MessageDialog(message.Message);
            messageDialog.Commands.Add(new UICommand(
                "Close"));
            await messageDialog.ShowAsync();
        }

        private async void ImportDataBase_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var messageDialog = new MessageDialog("Do you want to clear all listed games?");
                messageDialog.Commands.Add(new UICommand("Clear", new UICommandInvokedHandler(this.CommandInvokedHandler2)));
                messageDialog.Commands.Add(new UICommand("Cancel"));
                await messageDialog.ShowAsync();

            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }
        private async void CommandInvokedHandler2(IUICommand command)
        {
            try
            {
                if (GameListBox.Items != null && GameListBox.Items.Count > 0)
                {
                    ApplicationData.Current.LocalSettings.DeleteContainer("Games");
                    ApplicationData.Current.LocalSettings.DeleteContainer("Folders");
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                    {
                        try
                        {
                            GamePage.ShowTile(WidePreviewTile, 1500, "Clear List", "List cleared successfully");
                            ViewModel.ClearGames();
                            ShowDialog("Please restart the app before add any new games");
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        
        private async void CommandInvokedHandler3(IUICommand command)
        {
            try
            {
                var testFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ScummVM.dat");
                if (testFile != null)
                {
                    await testFile.DeleteAsync();
                    GamePage.ShowTile(WidePreviewTile, 1500, "Unset Database", $"Unset done");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void UnsetDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var testFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ScummVM.dat");
                if (testFile != null)
                {
                    var messageDialog = new MessageDialog("Do you need to unset the current custom database?");
                    messageDialog.Commands.Add(new UICommand("Unset", new UICommandInvokedHandler(this.CommandInvokedHandler3)));
                    messageDialog.Commands.Add(new UICommand("Cancel"));
                    await messageDialog.ShowAsync();
                }
                else
                {
                    ShowDialog("No database found!");
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }
    }
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input[0].ToString().ToUpper() + input.Substring(1);
            }
        }
    }
}
