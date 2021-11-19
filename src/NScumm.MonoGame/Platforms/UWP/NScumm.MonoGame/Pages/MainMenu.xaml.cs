using NScumm.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using System;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Storage;

namespace NScumm.MonoGame.Pages
{
    public sealed partial class MainMenu : ContentDialog
    {
        private ResourceLoader _loader;

        internal ScummGame Game { get; set; }

        private IEngine Engine
        {
            get
            {
                var engine = Game.Services.GetService<IEngine>();
                return engine;
            }
        }

        public MainMenu()
        {
            InitializeComponent();

            _loader = ResourceLoader.GetForViewIndependentUse();
        }

        private void OnResume(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }

        private void OnBack(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Title = _loader.GetString("MainMenu_Title");
            MainStackPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            LoadStackPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SaveStackPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void OnLoad(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Title = _loader.GetString("MainMenu_LoadTitle");
            LoadGameList.Items.Clear();
            var savegames = await GetSavegames();
            savegames.ForEach(LoadGameList.Items.Add);

            MainStackPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            LoadStackPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private async void OnSave(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Title = _loader.GetString("MainMenu_SaveTitle");
            SaveGameList.Items.Clear();
            var savegames = await GetSavegames();
            savegames.ForEach(SaveGameList.Items.Add);
            ListViewItem NewEntry = new ListViewItem();
            NewEntry.Content = _loader.GetString("New Entry");
            NewEntry.Background = new SolidColorBrush(Colors.DodgerBlue);
            NewEntry.Foreground = new SolidColorBrush(Colors.White);

            SaveGameList.Items.Add(NewEntry);

            MainStackPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SaveStackPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void OnExit(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ExitGame_Click();
            Hide();

        }
        private async void ExitGame_Click()
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
            Engine.IsPaused = false;
            Engine.HasToQuit = true;
            App.isGameStarted = false;
        }
        private void OnLoadGame(object sender, ItemClickEventArgs e)
        {
            //string ObjectType = e.ClickedItem.GetType().Name;
            var index = ((ListView)sender).Items.IndexOf(e.ClickedItem);
            LoadGame(index);
            Hide();
        }

        private void OnSaveGame(object sender, ItemClickEventArgs e)
        {
            var index = ((ListView)sender).Items.IndexOf(e.ClickedItem);
            if(index == -1)
            {
                index = ((ListView)sender).Items.Count - 1;
            }
            SaveGame(index);
            Hide();
        }


        private async Task<List<string>> GetSavegames()
        {
            var local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var spec = await local.CreateFolderAsync(Game.Settings.Game.Id, Windows.Storage.CreationCollisionOption.OpenIfExists);
            if (spec != null)
            {
                var dir = spec.Path;
                var pattern = string.Format("{0}*.sav", Game.Settings.Game.Id);
                return ServiceLocator.FileStorage.EnumerateFiles(dir, pattern).Select(Path.GetFileNameWithoutExtension).ToList();
            }
            return new List<string>();
        }

        private async Task<string> GetSaveGamePath(int index)
        {
            var local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var spec = await local.CreateFolderAsync(Game.Settings.Game.Id, Windows.Storage.CreationCollisionOption.OpenIfExists);
            if (spec != null)
            {
                var indexResolve = "00";
                if(index > 9)
                {
                    indexResolve = "0";
                }else if (index > 99)
                {
                    indexResolve = "";
                }
                var dir = spec.Path;
                var filename = Path.Combine(dir, string.Format("{0} ({1}{2}).sav", Game.Settings.Game.Id, indexResolve ,(index + 1)));
                return filename;
            }
            return "";
        }

        private async void LoadGame(int index)
        {
            try
            {
                var filename = await GetSaveGamePath(index);
                if (filename.Length > 0)
                {
                    Engine.Load(filename);
                    
                    GamePage.ShowTileHandler.Invoke(new string[] { "Load State", "State loaded successfully", $"Name: { Path.GetFileNameWithoutExtension(filename) }"}, EventArgs.Empty);
                }
            }catch(Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private async void SaveGame(int index)
        {
            try
            {
                var filename = await GetSaveGamePath(index);
                if (filename.Length > 0)
                {
                    Engine.Save(filename);
                    GamePage.ShowTileHandler.Invoke(new string[] { "Save State", "Save state done", $"Name: { Path.GetFileNameWithoutExtension(filename) }" }, EventArgs.Empty);
                }
            }catch(Exception ex)
            {
                ShowDialog(ex);
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
    }
}
