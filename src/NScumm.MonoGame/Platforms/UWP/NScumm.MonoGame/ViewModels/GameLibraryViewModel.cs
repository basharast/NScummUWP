//#define CLEAN_GAMELIST
//#define WP_GAMES

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Windows.Foundation;
using System.Reactive.Linq;
using Windows.Storage;
using Windows.Storage.AccessCache;
using System.Reactive.Concurrency;
using System.Windows.Input;
using Windows.Storage.Pickers;
using Microsoft.Xna.Framework.Input;
using NScumm.Core.IO;
using NScumm.Scumm.IO;
using NScumm.Sky;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace NScumm.MonoGame.ViewModels
{
    public interface IGameLibraryViewModel
    {
        bool IsScanning { get; }
        bool ShowNoGameMessage { get; set; }
        double LoadingProgressValue { get; set; }
        ObservableCollection<GameViewModel> Games { get; }

        void UpdateShowNoGameMessage();
        void ClearGames();

        ICommand AddCommand { get; }

    }

    class GameLibraryViewModel : ViewModel, IGameLibraryViewModel
    {
        static readonly HashSet<string> IndexFiles = new HashSet<string>(new[] { ".0", ".1", ".2", ".3", ".5", ".6", ".8", ".16", ".25", ".99", ".101", ".102", ".418", ".455", ".512", ".scummvm", ".scumm", ".gam", ".z5", ".dat", ".blb", ".z6", ".RAW", ".ROM", ".taf", ".zblorb", ".dcp", ".(a)", ".cup", ".HE0", ".(A)", ".D$$", ".STK", ".z8", ".hex", ".VMD", ".TGA", ".ITK", ".SCN", ".INF", ".pic", ".Z5", ".z3", ".blorb", ".ulx", ".DAT", ".cas", ".PIC", ".acd", ".006", ".SYS", ".alr", ".t3", ".gblorb", ".tab", ".AP", ".CRC", ".EXE", ".z4", ".W32", ".MAC", ".mac", ".WIN", ".001", ".003", ".000", ".bin", ".exe", ".asl", ".AVD", ".INI", ".SND", ".cat", ".ANG", ".CUP", ".SYS16", ".img", ".LB", ".TLK", ".MIX", ".VQA", ".RLB", ".FNT", ".win", ".HE1", ".DMU", ".FON", ".SCR", ".TEX", ".HEP", ".DIR", ".DRV", ".MAP", ".a3c", ".GRV", ".CUR", ".OPT", ".gfx", ".ASK", ".LNG", ".ini", ".RSC", ".SPP", ".CC", ".BND", ".LA0", ".TRS", ".add", ".HRS", ".DFW", ".DR1", ".ALD", ".004", ".002", ".005", ".R02", ".R00", ".C00", ".D00", ".GAM", ".IDX", ".ogg", ".TXT", ".GRA", ".BMV", ".H$$", ".MSG", ".VGA", ".PKD", ".OUT", ".99 (PG)", ".SAV", ".PAK", ".BIN", ".CPS", ".SHP", ".DXR", ".dxr", ".gmp", ".SNG", ".C35", ".C06", ".WAV", ".SMK", ".wav", ".CAB", ".game", ".Z6", ".(b)", ".slg", ".he2", ".he1", ".HE2", ".SYN", ".PAT", ".NUT", ".nl", ".PRC", ".V56", ".SEQ", ".P56", ".AUD", ".FKR", ".EX1", ".rom", ".LIC", ".$00", ".ALL", ".LTK", ".txt", ".acx", ".VXD", ".ACX", ".mpc", ".msd", ".ADF", ".nib", ".HELLO", ".dsk", ".xfd", ".woz", ".d$$", ".SET", ".SOL", ".Pat", ".CFG", ".BSF", ".RES", ".IMD", ".LFL", ".SQU", ".rsc", ".BBM", ".2 US", ".OVL", ".OVR", ".007", ".PNT", ".pat", ".CHK", ".MDT", ".EMC", ".ADV", ".FDT", ".GMC", ".FMC", ".info", ".HPF", ".hpf", ".INE", ".RBT", ".CSC", ".HEB", ".MID", ".lfl", ".LEC", ".HNM", ".QA", ".009", ".PRF", ".EGA", ".MHK", ".d64", ".prg", ".LZC", ".flac", ".IMS", ".REC", ".MOR", ".doc", ".HAG", ".AGA", ".BLB", ".TABLE", ".PAL", ".PRG", ".CLG", ".ORB", ".BRO", ".bro", ".PH1", ".DEF", ".IN", ".jpg", ".TOC", ".j2", ".Text", ".CEL", ".he0", ".AVI", ".1C", ".1c", ".BAK", ".L9", ".CGA", ".HRC", ".mhk", ".RED", ".SM0", ".SM1", ".SOU", ".RRM", ".LIB", ". Seuss's  ABC", ".CNV", ".VOC", ".OGG", ".GME", ".GERMAN", ".SHR", ".FRENCH", ".DNR", ".DSK", ".dnr", ".CAT", ".V16", ".cab", ".CLU", ".b25c", ".RL", ".mp3", ".FRM", ".SOG", ".HEX", ".mma", ".st", ".MPC", ".IMG", ".ENC", ".SPR", ".AD", ".C", ".CON", ".PGM", ".Z", ".RL2", ".MMM", ".OBJ", ".ZFS", ".zfs", ".STR", ".z2", ".z1" }, StringComparer.OrdinalIgnoreCase);
        private ObservableCollection<GameViewModel> _games;
        private readonly HashSet<string> _gameSignatures;
        private HashSet<string> _folders;
        private bool _isScanning;
        private bool _showNoGameMessage;
        private double _loadingProgressValue;
        private readonly DelegateCommand _addCommand;
        private ApplicationDataContainer _gamesContainer;
        private ApplicationDataContainer _foldersContainer;

        public ObservableCollection<GameViewModel> Games { get { return _games; } }

        public bool IsScanning
        {
            get { return _isScanning; }
            private set { RaiseAndSetIfChanged(ref _isScanning, value); }
        }

        public bool ShowNoGameMessage
        {
            get { return _showNoGameMessage; }
            set { RaiseAndSetIfChanged(ref _showNoGameMessage, value); }
        }

        public double LoadingProgressValue
        {
            get { return _loadingProgressValue; }
            set { RaiseAndSetIfChanged(ref _loadingProgressValue, value); }
        }

        public ICommand AddCommand
        {
            get { return _addCommand; }
        }

        public GameDetector GameDetector { get; }

        public GameLibraryViewModel()
        {
            _gameSignatures = new HashSet<string>();
            _games = new ObservableCollection<GameViewModel>();
            _addCommand = new DelegateCommand(Scan);

            GameDetector = new GameDetector();
            GameDetector.Add(new ScummMetaEngine());
            GameDetector.Add(new SkyMetaEngine());
            GameDetector.Add(new Sword1.Sword1MetaEngine());

            LoadGameLibrary();
        }

        private async void LoadGameLibrary()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                IsScanning = true;
            });
            UpdateShowNoGameMessage();

            LoadGameFolders();
            LoadGames();
        }

        private async void LoadGames()
        {
            await Task.Run(async () => {
                try
                {
                    _gamesContainer = ApplicationData.Current.LocalSettings.CreateContainer("Games", ApplicationDataCreateDisposition.Always);
                    var paths = (from gameContainer in _gamesContainer.Containers.Values
                                 let path = (string)gameContainer.Values["Path"]
                                 select path).ToList();

                    var TotalItems = paths.Count;
                    var TotalGamesScanned = 0;
                    List<GameDetected> gamesList = new List<GameDetected>();
                    foreach (var path in paths)
                    {
                        TotalGamesScanned++;
                        double progress = ((TotalGamesScanned * 1d) / (TotalItems * 1d)) * 100;
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                        {
                            LoadingProgressValue = progress;
                        });
                        try
                        {
                            //if (File.Exists(path))
                            {
                                var game = GameDetector.DetectGame(path);
                                if (game != null && !_gameSignatures.Contains(game.Game.Path))
                                {
                                    gamesList.Add(game);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    TotalItems = gamesList.Count;
                    TotalGamesScanned = 0;
                    foreach (var item in gamesList)
                    {
                        TotalGamesScanned++;
                        double progress = ((TotalGamesScanned * 1d) / (TotalItems * 1d)) * 100;
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                        {
                            LoadingProgressValue = progress;
                            _gameSignatures.Add(item.Game.Path);
                            _games.Add(new GameViewModel(item));
                        });
                    }

                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                    {
                        IsScanning = false;
                        LoadingProgressValue = 0;
                        UpdateShowNoGameMessage();
                    });
                }catch(Exception e)
                {
                    _ = MessageBox.Show("nSCUMM", e.Message, new[] { "OK" });
                }
            });
        }

        private void LoadGameFolders()
        {
            _foldersContainer = ApplicationData.Current.LocalSettings.CreateContainer("Folders", ApplicationDataCreateDisposition.Always);
            var folders = from folderContainer in _foldersContainer.Containers.Values
                          let path = (string)folderContainer.Values["Path"]
                          select path;
            _folders = new HashSet<string>(folders, StringComparer.OrdinalIgnoreCase);
        }

        private async void Scan()
        {
            var folder = await PickFolder();
            if (folder == null)
            {
                return;
            }

            IsScanning = true;

            UpdateShowNoGameMessage();
            List<GameDetected> indexes = new List<GameDetected>();
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            await Task.Run(async () =>
            {
                //Scan for games
                var obsItems = await GetFilesAsync(folder);
                var TotalGamesScanned = 0;
                var gamesCount = obsItems.Count;
                foreach (var item in obsItems)
                {
                    try
                    {
                        TotalGamesScanned++;
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                        {
                            try
                            {
                                double progress = ((TotalGamesScanned * 1d) / (gamesCount * 1d)) * 100;
                                LoadingProgressValue = progress;
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                        if (IndexFiles.Contains(Path.GetExtension(item.Path).ToLower()))
                        {
                            var g = GameDetector.DetectGame(item.Path);
                            if (g != null && !_gameSignatures.Contains(g.Game.Path))
                            {
                                indexes.Add(g);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                taskCompletionSource.SetResult(true);
            });
            await taskCompletionSource.Task;
            LoadingProgressValue = 0;

            foreach (var item in indexes)
            {
                try
                {
                    var name = string.Format("Game{0}", _gamesContainer.Containers.Count + 1);
                    var gameContainer = _gamesContainer.CreateContainer(name, ApplicationDataCreateDisposition.Always);
                    gameContainer.Values["Path"] = item.Game.Path;
                    _games.Add(new GameViewModel(item));
                }
                catch (Exception ex)
                {
                    await FileIO.AppendTextAsync(await ApplicationData.Current.LocalFolder.CreateFileAsync("log.txt", CreationCollisionOption.ReplaceExisting), ex.Message);
                }
            }

            IsScanning = false;
            UpdateShowNoGameMessage();
        }

        private void UpdateShowNoGameMessage()
        {
            ShowNoGameMessage = !IsScanning && _games.Count == 0;
        }

        private void AddFolder(StorageFolder folder)
        {
            if (folder == null) return;

            // check if the folder already existin the folder list
            if (!_folders.Contains(folder.Path))
            {
                _folders.Add(folder.Path);

                var token = string.Format("Folder{0}", _folders.Count);

                // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, folder);

                var gameFolder = _foldersContainer.CreateContainer(token, ApplicationDataCreateDisposition.Always);
                gameFolder.Values["Path"] = folder.Path;
            }
        }

        private async System.Threading.Tasks.Task<StorageFolder> PickFolder()
        {
            StorageFolder folder = null;
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            IndexFiles.ToList().ForEach(folderPicker.FileTypeFilter.Add);
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            try
            {
                folder = await folderPicker.PickSingleFolderAsync();
                AddFolder(folder);
            }
            catch (UnauthorizedAccessException)
            {
            }
            return folder;
        }

        private async Task<IReadOnlyList<StorageFile>> GetFilesAsync(StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            return files;
            /*var obsItems = folder.GetItemsAsync().ToObservable();
            var files = (from item in obsItems
                         from i in item
                         select i is StorageFile ? Observable.Return((StorageFile)i) : GetFilesAsync((StorageFolder)i))
                         .SelectMany(i => i);
            return files;*/
        }

        void IGameLibraryViewModel.UpdateShowNoGameMessage()
        {
            UpdateShowNoGameMessage();
        }

        public void ClearGames()
        {
            _games.Clear();
            UpdateShowNoGameMessage();
        }
    }
}
