using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json;
using NScumm.Core;
using NScumm.Core.Audio;
using NScumm.Core.IO;

namespace NScumm.Scumm.IO
{
    public class GameManager
    {
        XDocument _doc;
        static readonly XNamespace Namespace = "http://schemas.scemino.com/nscumm/2012/";

        public static GameManager Create(Stream stream)
        {
            var gm = new GameManager { _doc = ServiceLocator.FileStorage.LoadDocument(stream) };
            return gm;
        }

        public static string GamesDatabaseCache = "";
        public static string GamesCustomDatabaseCache = "";
        static bool LanguagesListReady = false;

        public GameInfo GetInfo(string path)
        {
            GameInfo info = null;
            var signature = ServiceLocator.FileStorage.GetSignature(path);
            var gameMd5 = (from md5 in _doc.Element(Namespace + "NScumm").Elements(Namespace + "MD5")
                           where (string)md5.Attribute("signature") == signature
                           select md5).FirstOrDefault();
            if (gameMd5 != null)
            {
                try
                {
                    var game = (from g in _doc.Element(Namespace + "NScumm").Elements(Namespace + "Game")
                                where (string)g.Attribute("id") == (string)gameMd5.Attribute("gameId")
                                where (string)g.Attribute("variant") == (string)gameMd5.Attribute("variant")
                                select g).FirstOrDefault();
                    var desc = (from d in _doc.Element(Namespace + "NScumm").Elements(Namespace + "Description")
                                where (string)d.Attribute("gameId") == (string)gameMd5.Attribute("gameId")
                                select (string)d.Attribute("text")).FirstOrDefault();
                    var attFeatures = gameMd5.Attribute("features");
                    var platformText = (string)gameMd5.Attribute("platform");
                    Platform platform = Platform.DOS;
                    try
                    {
                        platform = (Platform)Enum.Parse(typeof(Platform), platformText, true);
                    }
                    catch (Exception ex)
                    {

                    }
                    var features = ParseFeatures((string)attFeatures);
                    var attMusic = game.Attribute("music");
                    var music = ParseMusic((string)attMusic);
                    var lang = "en";
                    try
                    {
                        var test = new CultureInfo((string)gameMd5.Attribute("language"));
                        lang = (string)gameMd5.Attribute("language");
                    }
                    catch (Exception ex)
                    {

                    }
                    info = new GameInfo
                    {
                        MD5 = signature,
                        Platform = platform,
                        Path = path,
                        Id = (string)game.Attribute("id"),
                        Pattern = (string)game.Attribute("pattern"),
                        Variant = (string)game.Attribute("variant"),
                        Description = desc,
                        Version = (int)game.Attribute("version"),
                        Culture = new CultureInfo((string)gameMd5.Attribute("language")),
                        Features = features,
                        Music = music
                    };
                }
                catch (Exception ex)
                {

                }
            }

            //Try to search into ScummVM.dat built-in and custom (if exists)
            if (info == null)
            {
                if (!LanguagesListReady)
                {
                    try
                    {
                        LanguageHelpers.LanguagesSupported = JsonConvert.DeserializeObject<Language[]>(LanguagesJSON.LanguagesList);
                    }
                    catch (Exception ex)
                    {

                    }
                    LanguagesListReady = true;
                }

                if (GamesDatabaseCache.Length == 0)
                {
                    GamesDatabaseCache = ServiceLocator.FileStorage.ReadContent("Content\\ScummVM.dat");
                }

                if (GamesDatabaseCache.Length > 0)
                {
                    var searchPattern = $"name \".*\\s\\((?<gameEngine>.*)\\)\"\\s+description \"(?<gameDesc>.*) \\(.*\\)\"\\s+releaseyear \"\\d+\"\\s+developer \".*\"\\s+code \"(?<gameCode>.*)\"\\s+rom \\( name \"(?<fileName>.*)\" size \\d+ crc \\w+ md5 {signature}";

                    var m = Regex.Match(GamesDatabaseCache, searchPattern, RegexOptions.IgnoreCase);
                    if (m.Groups != null && m.Groups.Count > 0 && m.Groups["fileName"].Value.Length > 0)
                    {
                        var fileName = m.Groups["fileName"].Value;
                        var gameCode = m.Groups["gameCode"].Value;
                        var gameDesc = m.Groups["gameDesc"].Value;
                        var gameEngine = m.Groups["gameEngine"].Value;
                        var gameMD5 = signature;

                        Platform platform = Platform.DOS;
                        try
                        {
                            platform = (Platform)Enum.Parse(typeof(Platform), "DOS", true);
                        }
                        catch (Exception ex)
                        {

                        }
                        var gameId = "";
                        var gameVariant = "";
                        var gameVersion = 0;
                        var gameLanguage = new CultureInfo("en");

                        //Extra content
                        var features = ParseFeatures("");
                        var music = ParseMusic("");


                        //Parse values after set the default values
                        gameId = gameCode;
                        try
                        {
                            //Extract gameID
                            var gameIdData = gameCode.Split('-');
                            gameId = gameIdData[0];
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            //Extract platform
                            var platformString = gameEngine;
                            var gameEngineData = gameEngine.Split('/');
                            if (gameEngineData.Length > 0)
                            {
                                foreach (var gItem in gameEngineData)
                                {
                                    try
                                    {
                                        platform = (Platform)Enum.Parse(typeof(Platform), gItem, true);
                                        break;
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            //Extract language
                            bool langaugeFound = false;
                            //1. try to search in gameCode
                            var gameCodeData = gameCode.Split('-');
                            foreach (var cItem in gameCodeData)
                            {
                                if (!cItem.Equals("cd") && !cItem.Equals("st"))
                                {
                                    try
                                    {
                                        gameLanguage = new CultureInfo(cItem);
                                        langaugeFound = true;
                                        break;
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                            //2. try to search in game description
                            if (!langaugeFound)
                            {
                                var gameEngineData = gameEngine.Split('/');
                                foreach (var eItem in gameEngineData)
                                {
                                    try
                                    {
                                        var langTest = LanguageHelpers.GetLanguageByName(eItem);
                                        if (langTest != null)
                                        {
                                            gameLanguage = new CultureInfo(langTest.ISO639);
                                            langaugeFound = true;
                                            break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            //Extract game version (usually if available will be in game code)
                            var gameCodeData = gameCode.Split('-');
                            foreach (var cItem in gameCodeData)
                            {
                                try
                                {
                                    var versionTest = int.Parse(cItem);
                                    gameVersion = versionTest;
                                    break;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            //Extract game variant (it can be in the descrition or game in game code)
                            //It's very confusing to find it as it will look like language (short code)
                            bool variantFound = false;

                            //1. try to get it from description
                            //if the second entry was platform then the first is gameVariant
                            var gameEngineData = gameEngine.Split('/');
                            if (gameEngineData.Length > 1)
                            {
                                try
                                {
                                    var platformTest = (Platform)Enum.Parse(typeof(Platform), gameEngineData[1], true);
                                    gameVariant = gameEngineData[0];
                                    variantFound = true;
                                }
                                catch (Exception ex)
                                {

                                }
                            }

                            //2. try to get it from game code
                            //gameVariant usually will be the second if it's not language tag
                            if (!variantFound)
                            {
                                var gameCodeData = gameCode.Split('-');
                                if (gameCodeData.Length > 1)
                                {
                                    var tempVariant = "";
                                    var cItem = gameCodeData[1];
                                    if (!cItem.Equals("cd") && !cItem.Equals("st"))
                                    {
                                        try
                                        {
                                            var testLanguage = new CultureInfo(cItem);
                                        }
                                        catch (Exception ex)
                                        {
                                            tempVariant = gameCodeData[1];
                                        }
                                    }
                                    else
                                    {
                                        tempVariant = gameCodeData[1];
                                    }

                                    //We have to test the selected variant if it's not version
                                    try
                                    {
                                        var testNumber = int.Parse(tempVariant);
                                    }
                                    catch (Exception ex)
                                    {
                                        gameVariant = tempVariant;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        info = new GameInfo
                        {
                            MD5 = signature,
                            Platform = platform,
                            Path = path,
                            Id = gameId,
                            Pattern = fileName,
                            Variant = gameVariant,
                            Description = gameDesc,
                            Version = gameVersion,
                            Culture = gameLanguage,
                            Features = features,
                            Music = music
                        };
                    }
                }
            }

            return info;
        }

        GameFeatures ParseFeatures(string feature)
        {
            var features = GameFeatures.None;
            try
            {
                var feat = feature == null ? new string[0] : feature.Split(' ');
                foreach (var f in feat)
                {
                    try
                    {
                        features |= (GameFeatures)Enum.Parse(typeof(GameFeatures), f, true);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return features;
        }

        MusicDriverTypes ParseMusic(string music)
        {
            var musics = MusicDriverTypes.None;

            try
            {
                var mus = music == null ? new string[0] : music.Split(' ');
                foreach (var m in mus)
                {
                    try
                    {
                        musics |= (MusicDriverTypes)Enum.Parse(typeof(MusicDriverTypes), m, true);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return musics;
        }
    }
}
