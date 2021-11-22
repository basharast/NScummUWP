using MonoGame.Framework;
using NScumm.Core.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using NScumm.MonoGame.Services;
using System;
using Windows.UI.Core;
using NScumm.Core;
using Windows.UI.Popups;
using Windows.ApplicationModel.Core;
using NScumm.Core.Audio;
using Windows.UI.ViewManagement;
using NScumm.Scumm;
using System.Threading.Tasks;
using Windows.Storage;
using System.Threading;
using Windows.Storage.Pickers;
using System.IO;
using NotificationsVisualizerLibrary;
using Windows.UI.Notifications;
using NotificationsExtensions.Tiles;

namespace NScumm.MonoGame
{
    public sealed partial class GamePage
    {
        internal static GameDetected Info;

        private ScummGame _game;
        private readonly MenuService _menuService;
        public static EventHandler ShowTileHandler;
        public static EventHandler FPSHandler;
        private Timer FPSTimer;
        public GamePage()
        {
            InitializeComponent();
            App.isGameStarted = true;
            try
            {
                var currentView = SystemNavigationManager.GetForCurrentView();
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            catch (Exception e)
            {
            }
            try
            {
                _menuService = new MenuService(Dispatcher);
            }
            catch (Exception ex)
            {

            }
            ShowTileHandler += ShowTileHandlerCall;
            FPSHandler += FPSHandlerCall;

            
            ShowTile(WidePreviewTile, 3500, "Game Started", "Game successfully started", new string[] { "If blackscreen wait..", "The engine is loading" });
        }


        int frameRate = 0;
        public int FrameRate { get { int tempValue = frameRate; frameRate = 0; return tempValue; } set { frameRate = value; } }
        private void UpdateFrameRate()
        {
            Interlocked.Increment(ref frameRate);
        }

        private void callFPSTimer(bool startState = false)
        {
            try
            {
                FPSTimer?.Dispose();
                if (startState)
                {
                    FPSTimer = new Timer(delegate { UpdateFPSCounter(null, EventArgs.Empty); }, null, 0, 1000);
                }
            }
            catch (Exception e)
            {

            }
        }
        public async void UpdateFPSCounter(object sender, EventArgs e)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        FrameRateText.Text = FrameRate.ToString();
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
            {

            }
        }
        private void FPSHandlerCall(object sender, EventArgs args)
        {
            if (ShowFPS.IsChecked)
            {
                UpdateFrameRate();
            }
        }
        private void ShowTileHandlerCall(object sender, EventArgs args)
        {
            try
            {
                var senderArray = (string[])sender;
                ShowTile(WidePreviewTile, 2000, senderArray[0], senderArray[1], new string[] { (senderArray.Length > 2 ? senderArray[2] : "") });
            }
            catch (Exception ex)
            {

            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SavesBar.Visibility = Visibility.Visible;
            var info = e.Parameter as GameDetected;
            Info = info;
            try
            {
                _game = XamlGame<ScummGame>.Create("", Window.Current.CoreWindow, GamePanel);
                _menuService.Game = _game;
                App.currentGameID = _game.Settings.Game.Id;
                _game.Services.AddService<IMenuService>(_menuService);
            }
            catch (Exception ex)
            {

            }
            SavesBar.Visibility = Visibility.Collapsed;
        }


        private IEngine Engine
        {
            get
            {
                var engine = _game.Services.GetService<IEngine>();
                return engine;
            }
        }

        private void ToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            Engine.IsPaused = true;
            _menuService.ShowMenu();
        }

        private void PauseToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (PauseToggle.IsChecked.Value)
            {
                Engine.IsPaused = true;
            }
            else
            {
                Engine.IsPaused = false;
            }
        }

        private void SkipIntro_Click(object sender, RoutedEventArgs e)
        {
            //game.Settings.Game.
        }

        private async void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var messageDialog = new MessageDialog("Do you want to exit?");
                messageDialog.Commands.Add(new UICommand("Exit", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.Commands.Add(new UICommand("Dismiss"));
                await messageDialog.ShowAsync();
            }
            catch (Exception ex)
            {

            }
        }
        private void CommandInvokedHandler(IUICommand command)
        {
            Engine.IsPaused = true;
            _menuService.Game.Exit();
            App.isGameStarted = false;
        }

        bool IsPaused = false;
        private void PauseToggle_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            IsPaused = !IsPaused;
        }

        private void ToggleKeyboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InputPane pane = InputPane.GetForCurrentView();
                if (pane.Visible)
                {
                    var state = pane.TryHide();
                    if (!state)
                    {
                    }
                }
                else
                {
                    var state = pane.TryShow();
                    if (!state)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void GamePanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleMenu.Focus(FocusState.Unfocused);
            ExitGame.Focus(FocusState.Unfocused);
            ToggleKeyboard.Focus(FocusState.Unfocused);
            MenuBar.Focus(FocusState.Unfocused);
        }

        private async void ImportSaves_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                filePicker.FileTypeFilter.Add(".zip");
                var documentFile = await filePicker.PickSingleFileAsync();
                if (documentFile != null)
                {
                    SavesBar.Visibility = Visibility.Visible;
                    TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await archiverPlus.Decompress(documentFile, ApplicationData.Current.LocalFolder);
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
                    SavesBar.Visibility = Visibility.Collapsed;
                    ShowTile(WidePreviewTile, 1500, "Import Saves", "Import Done");
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
                SavesBar.Visibility = Visibility.Collapsed;
            }
        }
        ArchiverPlus archiverPlus = new ArchiverPlus();
        private async void ExportSaves_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var local = ApplicationData.Current.LocalFolder;
                var dir = (StorageFolder)await local.TryGetItemAsync(App.currentGameID);
                if (dir != null)
                {
                    var backupName = $"NSCUMM_{App.CurrentGameID.ToUpper()}_" + DateTime.Now.ToString().Replace("/", "_").Replace("\\", "_").Replace(":", "_").Replace(" ", "_");
                    var savePicker = new FileSavePicker
                    {
                        SuggestedStartLocation = PickerLocationId.Downloads,
                        SuggestedFileName = $"{backupName}"
                    };
                    var filedb = new[] { ".zip" };
                    savePicker.FileTypeChoices.Add("Archive", filedb);
                    var targetExportFile = await savePicker.PickSaveFileAsync(); ;

                    if (targetExportFile != null)
                    {
                        SavesBar.Visibility = Visibility.Visible;
                        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                        TaskCompletionSource<bool> taskCompletionSource2 = new TaskCompletionSource<bool>();
                        var ArchiverError = "";
                        await Task.Run(async () =>
                        {
                            try
                            {
                                await archiverPlus.Compress(dir, targetExportFile, System.IO.Compression.CompressionLevel.Optimal);
                                if (archiverPlus.log.Count > 0)
                                {
                                    ArchiverError = String.Join("\n", archiverPlus.log);
                                }
                            }
                            catch (Exception ex)
                            {
                                ArchiverError = ex.Message;
                            }
                            try
                            {
                                taskCompletionSource2.SetResult(true);
                            }
                            catch (Exception ex)
                            {

                            }
                        }, cancellationTokenSource.Token);
                        await taskCompletionSource2.Task;
                        SavesBar.Visibility = Visibility.Collapsed;
                        if (ArchiverError.Length > 0)
                        {
                            throw new Exception(ArchiverError);
                        }
                        ShowTile(WidePreviewTile, 1500, "Export Saves", "Export Done");
                    }
                }
                else
                {
                    ShowTile(WidePreviewTile, 2000, "Export Saves", "No saves found!");
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
                SavesBar.Visibility = Visibility.Collapsed;
            }
        }
        private async void ShowDialog(Exception message, bool log = false)
        {
            try
            {
                if (log)
                {
                    var logFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("log.txt", CreationCollisionOption.OpenIfExists);
                    await FileIO.AppendTextAsync(logFile, message.Message);
                }
            }
            catch (Exception ex)
            {

            }
            var messageDialog = new MessageDialog(message.Message);
            messageDialog.Commands.Add(new UICommand(
                "Close"));
            await messageDialog.ShowAsync();
        }
        private async void ShowDialog(string message, bool log = false)
        {
            try
            {
                if (log)
                {
                    var logFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("log.txt", CreationCollisionOption.OpenIfExists);
                    await FileIO.AppendTextAsync(logFile, message);
                }
            }
            catch (Exception ex)
            {

            }
            var messageDialog = new MessageDialog(message);
            messageDialog.Commands.Add(new UICommand(
                "Close"));
            await messageDialog.ShowAsync();
        }

        private async void ClearSaves_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var messageDialog = new MessageDialog("Do you want to delete all saves?");
                messageDialog.Commands.Add(new UICommand("Delete", new UICommandInvokedHandler(this.CommandInvokedHandler2)));
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
                var local = ApplicationData.Current.LocalFolder;
                var dir = (StorageFolder)await local.TryGetItemAsync(App.currentGameID);
                if (dir != null)
                {
                    SavesBar.Visibility = Visibility.Visible;
                    await dir.DeleteAsync();
                    ShowTile(WidePreviewTile, 1500, "Clean Saves", "All saves cleaned");
                }
                else
                {
                    ShowTile(WidePreviewTile, 2000, "Clean Saves", "No saves found!");
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
            SavesBar.Visibility = Visibility.Collapsed;
        }

        private async void QuickSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SavesBar.Visibility = Visibility.Visible;
                var filename = await GetSaveGamePath();
                if (filename.Length > 0)
                {
                    Engine.Save(filename);
                    ShowTile(WidePreviewTile, 1500, "Quick Save", "Quick save done");
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }

            SavesBar.Visibility = Visibility.Collapsed;
        }

        public static async void ShowTile(PreviewTile tile, int timeout, string title, string message, string[] description = null)
        {
            tile.CreateTileUpdater().Clear();
            tile.DisplayName = title;
            try
            {
                var uiSettings = new Windows.UI.ViewManagement.UISettings();
                var color = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
                if (color == Windows.UI.Colors.Black)
                {
                    tile.VisualElements.BackgroundColor = Windows.UI.Color.FromArgb(177, 50, 50, 50);
                }
                else
                {
                    tile.VisualElements.BackgroundColor = Windows.UI.Color.FromArgb(177, 0, 0, 0);
                }
            }
            catch (Exception ex)
            {
                tile.VisualElements.BackgroundColor = Windows.UI.Color.FromArgb(177, 0, 0, 0);
            }
            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.Square150x150Logo = new Uri("ms-appx:///Assets/Square150x150Logo.png");
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
            tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");

            // Commit the tile properties we changed
            await tile.UpdateAsync();

            tile.Opacity = 0;
            tile.Visibility = Visibility.Visible;
            for (double i = 0; i < 1;)
            {
                tile.Opacity = i;
                i += 0.1;
                await Task.Delay(10);
            }

            await Task.Delay(500);
            // Using NotificationsExtensions.Win10 NuGet package
            TileBindingContentAdaptive bindingContent = new TileBindingContentAdaptive();
            // Add the date header
            bindingContent.Children.Add(new TileText()
            {
                Text = message
            });

            if (description != null && description.Length > 0)
            {
                foreach (var dItem in description)
                {
                    bindingContent.Children.Add(new TileText()
                    {
                        Text = dItem,
                        Style = TileTextStyle.CaptionSubtle
                    });
                }
            }

            TileBinding binding = new TileBinding()
            {
                Content = bindingContent
            };
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = binding,
                    TileWide = binding,
                    Branding = TileBranding.NameAndLogo
                }
            };

            tile.CreateTileUpdater().Update(new TileNotification(content.GetXml()));

            await Task.Delay(timeout);
            for (double i = 1; i > 0;)
            {
                tile.Opacity = i;
                i -= 0.1;
                await Task.Delay(10);
            }
            tile.Visibility = Visibility.Collapsed;
        }


        private async void QuickLoad_Click(object sender, RoutedEventArgs e)
        {
            var filename = await GetSaveGamePath(false);
            if (filename.Length > 0)
            {
                Engine.Load(filename);
                ShowTile(WidePreviewTile, 1500, "Quick Load", "Quick load done");
            }
            else
            {
                ShowTile(WidePreviewTile, 2000, "Quick Load", "No quick save found!");
            }
        }
        private async Task<string> GetSaveGamePath(bool save = true)
        {
            var local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var spec = await local.CreateFolderAsync(_game.Settings.Game.Id, Windows.Storage.CreationCollisionOption.OpenIfExists);
            if (spec != null)
            {
                var savename = string.Format("_{0}{1}.sav", _game.Settings.Game.Id, " (Quick)");
                var testFile = await spec.TryGetItemAsync(savename);
                if (testFile != null || save)
                {
                    var dir = spec.Path;
                    var filename = Path.Combine(dir, savename);
                    return filename;
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        private void ShowInfo_Click(object sender, RoutedEventArgs e)
        {
            ShowTile(WidePreviewTile, 3000, "Starting Game", $"{_game.Settings.Game.Description}", new string[] { $"Id: {_game.Settings.Game.Id}", $"Platform: {_game.Settings.Game.Platform}", $"Pixel: {_game.Settings.Game.PixelFormat}" });
        }

        private void ShowFPS_Click(object sender, RoutedEventArgs e)
        {
            callFPSTimer(ShowFPS.IsChecked);
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            var applicationView = ApplicationView.GetForCurrentView();
            if (FullScreen.IsChecked)
            {
                if (!applicationView.IsFullScreenMode)
                {
                    applicationView.TryEnterFullScreenMode();
                }
                else
                {
                    applicationView.ExitFullScreenMode();
                }
            }
            else
            {
                if (applicationView.IsFullScreenMode)
                {
                    applicationView.ExitFullScreenMode();
                }
                else
                {
                    applicationView.TryEnterFullScreenMode();
                }
            }
        }
    }
}
