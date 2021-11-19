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

namespace NScumm.MonoGame
{
    public sealed partial class GameLibraryPage
    {
        public IGameLibraryViewModel ViewModel
        {
            get { return DataContext as IGameLibraryViewModel; }
            set { DataContext = value; }
        }

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
            ShowDialog("1-Extract game content to folder\n2-Click add and select the folder\n3-Enjoy\n\nShortcuts\nF5: Show Menu\nSpace: Pause\nEsc: Skip\n\nCreated by scemino\nEnhanced by Bashar Astifan\nGitHub: https://github.com/basharast/NScummEmulator");
        }

        private async void ImportDataBase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ShowNotice("Remember, it will replace your previous database (if imported before)");
                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                filePicker.FileTypeFilter.Add(".xml");
                var documentFile = await filePicker.PickSingleFileAsync();
                if (documentFile != null)
                {
                    TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                    await Task.Run(async () =>
                    {
                        try
                        {
                         await documentFile.CopyAsync(ApplicationData.Current.LocalFolder, "Nscumm.xml", NameCollisionOption.ReplaceExisting);
                         GamePage.ShowTile(WidePreviewTile, 1500, "Import Database", "Database imported");
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
    }
}
