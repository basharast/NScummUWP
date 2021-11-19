using NScumm.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace NScumm.Platform_UWP
{
    public class SaveFileManager : ISaveFileManager
    {
        private IFileStorage _fileStorage;
        private string gameID = "";
        public SaveFileManager(IFileStorage fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public void setID(string id)
        {
            gameID = id;
        }

        public async Task<string[]> ListSavefiles(string pattern)
        {
            var path = await GetSavePath();
            return Directory.EnumerateFiles(path, pattern).Select(Path.GetFileName).ToArray();
        }

        public async Task<Stream> OpenForLoading(string fileName)
        {
            var path = await GetSavePath();
            return File.OpenRead(Path.Combine(path, fileName));
        }

        public async Task<Stream> OpenForSaving(string fileName, bool compress = true)
        {
            var path = await GetSavePath();
            return File.OpenWrite(Path.Combine(path, fileName));
        }

        private async Task<string> GetSavePath()
        {
            var local = ApplicationData.Current.LocalFolder;
            var dir = await local.CreateFolderAsync(gameID, CreationCollisionOption.OpenIfExists);
            return dir.Path;
        }
    }
}
