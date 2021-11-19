//
//  AppleII_SoundFunction5_Noise.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2015 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Diagnostics;
using NScumm.Scumm.Audio.Players;

namespace NScumm.Scumm.Audio.AppleII
{
    /// <summary>
    /// SoundFunction5: periodic noise
    /// </summary>
    class AppleII_SoundFunction5_Noise : IAppleII_SoundFunction
    {
        public void Init(Player_AppleII player, byte[] args)
        {
            _player = player;
            _index = 0;
            _param0 = args[0];
            Debug.Assert(_param0 > 0);
        }

        public bool Update()
        { // D222
            byte[] noiseMask =
            {
                0x3F, 0x3F, 0x7F, 0x7F, 0x7F, 0x7F, 0xFF, 0xFF, 0xFF, 0x0F, 0x0F
            };

            // while (i = 0; i < 10; ++i)
            if (_index < 10)
            {
                int count = _param0;
                do
                {
                    _update(Noise() & noiseMask[_index], 1);
                    --count;
                } while (count > 0);

                ++_index;
                return false;
            }

            return true;
        }

        void _update(int interval /*a*/, int count)
        { // D270
            Debug.Assert(count > 0); // 0 == 256?
            if (interval == 0)
                interval = 256;

            for (int i = count; i > 0; --i)
            {
                _player.GenerateSamples(10 + 5 * interval);
                _player.SpeakerToggle();

                _player.GenerateSamples(5 + 5 * interval);
                _player.SpeakerToggle();
            }
        }

        byte /*a*/ Noise()
        { // D261
            byte result = _noiseTable[_pos];
            _pos = (_pos + 1) % 256;
            return result;
        }

        Player_AppleII _player;
        int _pos;
        int _index;
        int _param0;

        static readonly byte[] _noiseTable =
        {
            0x65, 0x1b, 0xda, 0x11, 0x61, 0xe5, 0x77, 0x57, 0x92, 0xc8, 0x51, 0x1c, 0xd4, 0x91, 0x62, 0x63,
            0x00, 0x38, 0x57, 0xd5, 0x18, 0xd8, 0xdc, 0x40, 0x03, 0x86, 0xd3, 0x2f, 0x10, 0x11, 0xd8, 0x3c,
            0xbe, 0x00, 0x19, 0xc5, 0xd2, 0xc3, 0xca, 0x34, 0x00, 0x28, 0xbf, 0xb9, 0x18, 0x20, 0x01, 0xcc,
            0xda, 0x08, 0xbc, 0x75, 0x7c, 0xb0, 0x8d, 0xe0, 0x09, 0x18, 0xbf, 0x5d, 0xe9, 0x8c, 0x75, 0x64,
            0xe5, 0xb5, 0x5d, 0xe0, 0xb7, 0x7d, 0xe9, 0x8c, 0x55, 0x65, 0xc5, 0xb5, 0x5d, 0xd8, 0x09, 0x0d,
            0x64, 0xf0, 0xf0, 0x08, 0x63, 0x03, 0x00, 0x55, 0x35, 0xc0, 0x00, 0x20, 0x74, 0xa5, 0x1e, 0xe3,
            0x00, 0x06, 0x3c, 0x52, 0xd1, 0x70, 0xd0, 0x57, 0x02, 0xf0, 0x00, 0xb6, 0xfc, 0x02, 0x11, 0x9a,
            0x3b, 0xc8, 0x38, 0xdf, 0x1a, 0xb0, 0xd1, 0xb8, 0xd0, 0x18, 0x8a, 0x4a, 0xea, 0x1b, 0x12, 0x5d,
            0x29, 0x58, 0xd8, 0x43, 0xb8, 0x2d, 0xd2, 0x61, 0x10, 0x3c, 0x0c, 0x5d, 0x1b, 0x61, 0x10, 0x3c,
            0x0a, 0x5d, 0x1d, 0x61, 0x10, 0x3c, 0x0b, 0x19, 0x88, 0x21, 0xc0, 0x21, 0x07, 0x00, 0x65, 0x62,
            0x08, 0xe9, 0x36, 0x40, 0x20, 0x41, 0x06, 0x00, 0x20, 0x00, 0x00, 0xed, 0xa3, 0x00, 0x88, 0x06,
            0x98, 0x01, 0x5d, 0x7f, 0x02, 0x1d, 0x78, 0x03, 0x60, 0xcb, 0x3a, 0x01, 0xbd, 0x78, 0x02, 0x5d,
            0x7e, 0x03, 0x1d, 0xf5, 0xa6, 0x40, 0x81, 0xb4, 0xd0, 0x8d, 0xd3, 0xd0, 0x6d, 0xd5, 0x61, 0x48,
            0x61, 0x4d, 0xd1, 0xc8, 0xb1, 0xd8, 0x69, 0xff, 0x61, 0xd9, 0xed, 0xa0, 0xfe, 0x19, 0x91, 0x37,
            0x19, 0x37, 0x00, 0xf1, 0x00, 0x01, 0x1f, 0x00, 0xad, 0xc1, 0x01, 0x01, 0x2e, 0x00, 0x40, 0xc6,
            0x7a, 0x9b, 0x95, 0x43, 0xfc, 0x18, 0xd2, 0x9e, 0x2a, 0x5a, 0x4b, 0x2a, 0xb6, 0x87, 0x30, 0x6c
        };
    }
}
