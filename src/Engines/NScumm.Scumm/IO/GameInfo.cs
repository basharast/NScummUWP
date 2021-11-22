using System;
using System.Globalization;
using NScumm.Core.Audio;
using NScumm.Core.Graphics;
using NScumm.Core.IO;

namespace NScumm.Scumm.IO
{
    [Flags]
    public enum GameFeatures
    {
        None,
        SixteenColors = 0x01,
        Old256 = 0x02,
        FewLocals = 0x04,
        Demo = 0x08,
        Is16BitColor = 0x10,
        AudioTracks = 0x20,
    }

    /* GameId is not in use anymore, use Id instead */
    public enum GameId
    { 
        None
    }

    public class GameInfo : IGameDescriptor
    {
        public Platform Platform { get; set; }

        public string Path { get; set; }

        public string Id { get; set; }

        public string Pattern { get; set; }

        /*  GameId not in use anymore */
        public GameId GameId { get; set; }

        public string Variant { get; set; }

        public string Description { get; set; }

        public string MD5 { get; set; }

        public int Version { get; set; }

        public CultureInfo Culture { get; set; }

        public GameFeatures Features { get; set; }

        public MusicDriverTypes Music { get; set; }

        public bool IsOldBundle { get { return Version <= 3 && Features.HasFlag(GameFeatures.SixteenColors); } }

        public int Width
        {
            get
            {
                return Version == 8 ? 640 : 320;
            }
        }

        public int Height
        {
            get
            {
                if (Platform == Platform.FMTowns && Version == 3)
                {
                    return 240;
                }
                return Version == 8 ? 480 : 200;
            }
        }

        public PixelFormat PixelFormat
        {
            get
            {
                var format = Platform == Platform.FMTowns ? PixelFormat.Rgb16 : PixelFormat.Indexed8;
                return format;
            }
        }
    }
}