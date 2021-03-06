//
//  CharsetRenderer2.cs
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

using System;

namespace NScumm.Scumm.Graphics
{
    public class CharsetRenderer2: CharsetRenderer3
    {
        public CharsetRenderer2(ScummEngine vm)
            : base(vm)
        {
            _fontHeight = 8;
            CurId = 0;

            byte[] replacementData = null;
            int replacementChars = 0;

            switch (vm.Game.Culture.TwoLetterISOLanguageName)
            {
                case "de":
                    if (vm.Game.Version == 0)
                    {
                        replacementData = germanCharsetDataV0;
                        replacementChars = germanCharsetDataV0.Length / 2;
                    }
                    else
                    {
                        replacementData = germanCharsetDataV2;
                        replacementChars = germanCharsetDataV2.Length / 2;
                    }
                    break;
                case "fr":
                    replacementData = frenchCharsetDataV2;
                    replacementChars = frenchCharsetDataV2.Length / 2;
                    break;
                case "it":
                    replacementData = italianCharsetDataV2;
                    replacementChars = italianCharsetDataV2.Length / 2;
                    break;
                case "es":
                    replacementData = spanishCharsetDataV2;
                    replacementChars = spanishCharsetDataV2.Length / 2;
                    break;
                case "ru":
                    _fontPtr = russianCharsetDataV2;
                    break;
                default:
                    _fontPtr = englishCharsetDataV2;
                    break;
            }

            if (replacementData != null)
            {
                _fontPtr = new byte[englishCharsetDataV2.Length];
                Array.Copy(englishCharsetDataV2, _fontPtr, englishCharsetDataV2.Length);

                for (int i = 0; i < replacementChars; i++)
                {
                    int ch1 = replacementData[2 * i];
                    int ch2 = replacementData[2 * i + 1];

                    Array.Copy(specialCharsetData, 8 * ch2, _fontPtr, 8 * ch1, 8);
                }
            }
        }

        public override void SetCurID(int id)
        {
        }

        public override int GetCharWidth(int chr)
        {
            return 8;
        }

        // English Zak font
        static readonly byte[] englishCharsetDataV2 =
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x01, 0x03, 0x06, 0x0C, 0x18, 0x3E, 0x03, 0x00,
                0x80, 0xC0, 0x60, 0x30, 0x18, 0x7C, 0xC0, 0x00,
                0x00, 0x03, 0x3E, 0x18, 0x0C, 0x06, 0x03, 0x01,
                0x00, 0xC0, 0x7C, 0x18, 0x30, 0x60, 0xC0, 0x80,
                0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03,
                0x03, 0x03, 0x03, 0x07, 0x07, 0x0F, 0x1F, 0x7F,
                0xE0, 0x80, 0x00, 0x00, 0x00, 0x00, 0x80, 0xE0,
                0x07, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x07,
                0x00, 0x00, 0x00, 0x01, 0x03, 0x06, 0x0C, 0x18,
                0x00, 0x00, 0x00, 0x80, 0xC0, 0x60, 0x30, 0x18,
                0x18, 0x30, 0x60, 0xC0, 0x80, 0x00, 0x00, 0x00,
                0x18, 0x0C, 0x06, 0x03, 0x01, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x1F, 0x1F, 0x18, 0x18, 0x18,
                0x00, 0x00, 0x00, 0xF8, 0xF8, 0x18, 0x18, 0x18,
                0x18, 0x18, 0x18, 0xF8, 0xF8, 0x00, 0x00, 0x00,
                0x18, 0x18, 0x18, 0x1F, 0x1F, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x03, 0x07, 0x0C, 0x18, 0x18,
                0x00, 0x00, 0x00, 0xC0, 0xE0, 0x30, 0x18, 0x18,
                0x18, 0x18, 0x30, 0xE0, 0xC0, 0x00, 0x00, 0x00,
                0x18, 0x18, 0x0C, 0x07, 0x03, 0x00, 0x00, 0x00,
                0x18, 0x18, 0x18, 0x1F, 0x1F, 0x18, 0x18, 0x18,
                0x18, 0x18, 0x18, 0xF8, 0xF8, 0x18, 0x18, 0x18,
                0x18, 0x18, 0x18, 0xFF, 0xFF, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0xFF, 0xFF, 0x18, 0x18, 0x18,
                0x18, 0x18, 0x18, 0xFF, 0xFF, 0x18, 0x18, 0x18,
                0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18,
                0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00,
                0x18, 0x3C, 0x66, 0xC3, 0xC3, 0x66, 0x3C, 0x18,
                0x18, 0x3C, 0x7E, 0xFF, 0xFF, 0x7E, 0x3C, 0x18,
                0x18, 0x66, 0xC3, 0xDB, 0xDB, 0xC3, 0x66, 0x18,
                0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x18, 0x18, 0x18, 0x18, 0x00, 0x00, 0x18, 0x00,
                0x66, 0x66, 0x66, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x66, 0x66, 0xFF, 0x66, 0xFF, 0x66, 0x66, 0x00,
                0x18, 0x3E, 0x58, 0x3C, 0x1A, 0x7C, 0x18, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x49, 0x49, 0x00,
                0x3C, 0x66, 0x3C, 0x38, 0x67, 0x66, 0x3F, 0x00,
                0x06, 0x0C, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x0C, 0x18, 0x30, 0x30, 0x30, 0x18, 0x0C, 0x00,
                0x30, 0x18, 0x0C, 0x0C, 0x0C, 0x18, 0x30, 0x00,
                0x00, 0x66, 0x3C, 0xFF, 0x3C, 0x66, 0x00, 0x00,
                0x00, 0x18, 0x18, 0x7E, 0x18, 0x18, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x18, 0x30,
                0x00, 0x00, 0x00, 0x7E, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x18, 0x00,
                0x00, 0x03, 0x06, 0x0C, 0x18, 0x30, 0x60, 0x00,
                0x3C, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x00,
                0x18, 0x18, 0x38, 0x18, 0x18, 0x18, 0x7E, 0x00,
                0x3C, 0x66, 0x06, 0x0C, 0x30, 0x60, 0x7E, 0x00,
                0x3C, 0x66, 0x06, 0x1C, 0x06, 0x66, 0x3C, 0x00,
                0x06, 0x0E, 0x1E, 0x66, 0x7F, 0x06, 0x06, 0x00,
                0x7E, 0x60, 0x7C, 0x06, 0x06, 0x66, 0x3C, 0x00,
                0x3C, 0x66, 0x60, 0x7C, 0x66, 0x66, 0x3C, 0x00,
                0x7E, 0x66, 0x0C, 0x18, 0x18, 0x18, 0x18, 0x00,
                0x3C, 0x66, 0x66, 0x3C, 0x66, 0x66, 0x3C, 0x00,
                0x3C, 0x66, 0x66, 0x3E, 0x06, 0x66, 0x3C, 0x00,
                0x00, 0x00, 0x18, 0x00, 0x00, 0x18, 0x00, 0x00,
                0x00, 0x00, 0x18, 0x00, 0x00, 0x18, 0x18, 0x30,
                0x0E, 0x18, 0x30, 0x60, 0x30, 0x18, 0x0E, 0x00,
                0x7C, 0x82, 0xBA, 0xA2, 0xBA, 0x82, 0x7C, 0x00,
                0x70, 0x18, 0x0C, 0x06, 0x0C, 0x18, 0x70, 0x00,
                0x3C, 0x66, 0x06, 0x0C, 0x18, 0x00, 0x18, 0x00,
                0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00,
                0x18, 0x3C, 0x66, 0x7E, 0x66, 0x66, 0x66, 0x00,
                0x7C, 0x66, 0x66, 0x7C, 0x66, 0x66, 0x7C, 0x00,
                0x3C, 0x66, 0x60, 0x60, 0x60, 0x66, 0x3C, 0x00,
                0x78, 0x6C, 0x66, 0x66, 0x66, 0x6C, 0x78, 0x00,
                0x7E, 0x60, 0x60, 0x78, 0x60, 0x60, 0x7E, 0x00,
                0x7E, 0x60, 0x60, 0x78, 0x60, 0x60, 0x60, 0x00,
                0x3C, 0x66, 0x60, 0x6E, 0x66, 0x66, 0x3C, 0x00,
                0x66, 0x66, 0x66, 0x7E, 0x66, 0x66, 0x66, 0x00,
                0x3C, 0x18, 0x18, 0x18, 0x18, 0x18, 0x3C, 0x00,
                0x1E, 0x0C, 0x0C, 0x0C, 0x0C, 0x6C, 0x38, 0x00,
                0x66, 0x6C, 0x78, 0x70, 0x78, 0x6C, 0x66, 0x00,
                0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x7E, 0x00,
                0x63, 0x77, 0x7F, 0x6B, 0x63, 0x63, 0x63, 0x00,
                0x66, 0x76, 0x7E, 0x7E, 0x6E, 0x66, 0x66, 0x00,
                0x3C, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x00,
                0x7C, 0x66, 0x66, 0x7C, 0x60, 0x60, 0x60, 0x00,
                0x3C, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x0E, 0x00,
                0x7C, 0x66, 0x66, 0x7C, 0x78, 0x6C, 0x66, 0x00,
                0x3C, 0x66, 0x60, 0x3C, 0x06, 0x66, 0x3C, 0x00,
                0x7E, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x00,
                0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x00,
                0x66, 0x66, 0x66, 0x66, 0x66, 0x3C, 0x18, 0x00,
                0x63, 0x63, 0x63, 0x6B, 0x7F, 0x77, 0x63, 0x00,
                0x66, 0x66, 0x3C, 0x18, 0x3C, 0x66, 0x66, 0x00,
                0x66, 0x66, 0x66, 0x3C, 0x18, 0x18, 0x18, 0x00,
                0x7E, 0x06, 0x0C, 0x18, 0x30, 0x60, 0x7E, 0x00,
                0x3C, 0x30, 0x30, 0x30, 0x30, 0x30, 0x3C, 0x00,
                0x0C, 0x12, 0x30, 0x7C, 0x30, 0x62, 0xFC, 0x00,
                0x3C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x3C, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0xDB, 0xDB, 0x00,
                0x00, 0x10, 0x30, 0x7F, 0x7F, 0x30, 0x10, 0x00,
                0x66, 0x66, 0x66, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x3C, 0x06, 0x3E, 0x66, 0x3E, 0x00,
                0x00, 0x60, 0x60, 0x7C, 0x66, 0x66, 0x7C, 0x00,
                0x00, 0x00, 0x3C, 0x60, 0x60, 0x60, 0x3C, 0x00,
                0x00, 0x06, 0x06, 0x3E, 0x66, 0x66, 0x3E, 0x00,
                0x00, 0x00, 0x3C, 0x66, 0x7E, 0x60, 0x3C, 0x00,
                0x00, 0x0E, 0x18, 0x3E, 0x18, 0x18, 0x18, 0x00,
                0x00, 0x00, 0x3E, 0x66, 0x66, 0x3E, 0x06, 0x7C,
                0x00, 0x60, 0x60, 0x7C, 0x66, 0x66, 0x66, 0x00,
                0x00, 0x18, 0x00, 0x38, 0x18, 0x18, 0x3C, 0x00,
                0x00, 0x06, 0x00, 0x06, 0x06, 0x06, 0x06, 0x3C,
                0x00, 0x60, 0x60, 0x6C, 0x78, 0x6C, 0x66, 0x00,
                0x00, 0x38, 0x18, 0x18, 0x18, 0x18, 0x3C, 0x00,
                0x00, 0x00, 0x66, 0x7F, 0x7F, 0x6B, 0x63, 0x00,
                0x00, 0x00, 0x7C, 0x66, 0x66, 0x66, 0x66, 0x00,
                0x00, 0x00, 0x3C, 0x66, 0x66, 0x66, 0x3C, 0x00,
                0x00, 0x00, 0x7C, 0x66, 0x66, 0x7C, 0x60, 0x60,
                0x00, 0x00, 0x3E, 0x66, 0x66, 0x3E, 0x06, 0x06,
                0x00, 0x00, 0x7C, 0x66, 0x60, 0x60, 0x60, 0x00,
                0x00, 0x00, 0x3E, 0x60, 0x3C, 0x06, 0x7C, 0x00,
                0x00, 0x18, 0x7E, 0x18, 0x18, 0x18, 0x0E, 0x00,
                0x00, 0x00, 0x66, 0x66, 0x66, 0x66, 0x3E, 0x00,
                0x00, 0x00, 0x66, 0x66, 0x66, 0x3C, 0x18, 0x00,
                0x00, 0x00, 0x63, 0x6B, 0x7F, 0x3E, 0x36, 0x00,
                0x00, 0x00, 0x66, 0x3C, 0x18, 0x3C, 0x66, 0x00,
                0x00, 0x00, 0x66, 0x66, 0x66, 0x3E, 0x0C, 0x78,
                0x00, 0x00, 0x7E, 0x0C, 0x18, 0x30, 0x7E, 0x00,
                0x01, 0x03, 0x06, 0x6C, 0x78, 0x70, 0x60, 0x00,
                0x18, 0x3C, 0x7E, 0xFF, 0x18, 0x18, 0x18, 0x18,
                0x18, 0x18, 0x18, 0x18, 0xFF, 0x7E, 0x3C, 0x18,
                0x10, 0x30, 0x70, 0xFF, 0xFF, 0x70, 0x30, 0x10,
                0x08, 0x0C, 0x0E, 0xFF, 0xFF, 0x0E, 0x0C, 0x08,
            };


        static readonly byte[] russianCharsetDataV2 =
            {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x80, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc, 0xc0, 0x80,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x7f, 0x7f, 0x7f, 0x7f, 0x7f, 0x7e, 0x7c,
            0x7c, 0x7e, 0x7f, 0x7e, 0x7e, 0x7e, 0x7e, 0x00,
            0x00, 0xfe, 0xfe, 0xfe, 0xfc, 0xfe, 0x7e, 0x3e,
            0x3e, 0x7e, 0xfe, 0x7e, 0x7e, 0x7e, 0x7e, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x1c, 0x20, 0x20, 0x20, 0x1c, 0x00,
            0x04, 0x04, 0x3c, 0x44, 0x44, 0x44, 0x3c, 0x00,
            0x00, 0x00, 0x38, 0x44, 0x78, 0x40, 0x3c, 0x00,
            0x0c, 0x10, 0x38, 0x10, 0x10, 0x10, 0x10, 0x00,
            0x00, 0x00, 0x38, 0x44, 0x44, 0x3c, 0x04, 0x38,
            0x00, 0x00, 0x38, 0x04, 0x3c, 0x44, 0x3c, 0x00,
            0x40, 0x40, 0x78, 0x44, 0x44, 0x44, 0x78, 0x00,
            0x02, 0x02, 0x38, 0x40, 0x40, 0x40, 0x38, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xfc, 0xfc,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x60, 0x60, 0x60, 0x60, 0x00, 0x00, 0x60, 0x00,
            0xcc, 0xcc, 0xcc, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x66, 0x66, 0xff, 0x66, 0xff, 0x66, 0x66, 0x00,
            0x30, 0x7c, 0xc0, 0x78, 0x0c, 0xf8, 0x30, 0x00,
            0xc4, 0xcc, 0x18, 0x30, 0x60, 0xcc, 0x8c, 0x00,
            0x78, 0xcc, 0x78, 0x70, 0xce, 0xcc, 0x7e, 0x00,
            0x60, 0x60, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x30, 0x60, 0xc0, 0xc0, 0xc0, 0x60, 0x30, 0x00,
            0xc0, 0x60, 0x30, 0x30, 0x30, 0x60, 0xc0, 0x00,
            0x00, 0x80, 0xc0, 0xc0, 0xc0, 0xc0, 0x80, 0x00,
            0x66, 0xcc, 0xcc, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x60, 0xc0,
            0x00, 0x00, 0x00, 0xf8, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x60, 0x00,
            0x66, 0x66, 0xcc, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x78, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0x78, 0x00,
            0x60, 0xe0, 0x60, 0x60, 0x60, 0x60, 0x60, 0x00,
            0x78, 0xcc, 0x0c, 0x18, 0x60, 0xc0, 0xfc, 0x00,
            0x78, 0xcc, 0x0c, 0x38, 0x0c, 0xcc, 0x78, 0x00,
            0x0c, 0x1c, 0x2c, 0x4c, 0xfe, 0x0c, 0x0c, 0x00,
            0xfc, 0xc0, 0xf8, 0x0c, 0x0c, 0xcc, 0x78, 0x00,
            0x78, 0xcc, 0xc0, 0xf8, 0xcc, 0xcc, 0x78, 0x00,
            0xfc, 0xcc, 0x18, 0x30, 0x30, 0x30, 0x30, 0x00,
            0x78, 0xcc, 0xcc, 0x78, 0xcc, 0xcc, 0x78, 0x00,
            0x78, 0xcc, 0xcc, 0x7c, 0x0c, 0xcc, 0x78, 0x00,
            0x00, 0x00, 0x60, 0x60, 0x00, 0x60, 0x60, 0x00,
            0x00, 0x00, 0x60, 0x60, 0x00, 0x60, 0x60, 0xc0,
            0x7c, 0xfe, 0xfc, 0x80, 0x80, 0x80, 0x80, 0x80,
            0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x3f, 0x61, 0xcc, 0xd0, 0xd0, 0xcc, 0x61, 0x3f,
            0x78, 0xcc, 0x0c, 0x18, 0x30, 0x00, 0x30, 0x00,
            0x00, 0x00, 0x00, 0xf8, 0xf8, 0x00, 0x00, 0x00,
            0x30, 0x78, 0xcc, 0xfc, 0xcc, 0xcc, 0xcc, 0x00,
            0xf8, 0xcc, 0xcc, 0xf8, 0xcc, 0xcc, 0xf8, 0x00,
            0x78, 0xcc, 0xc0, 0xc0, 0xc0, 0xcc, 0x78, 0x00,
            0xf0, 0xd8, 0xcc, 0xcc, 0xcc, 0xd8, 0xf0, 0x00,
            0xfc, 0xc0, 0xc0, 0xf8, 0xc0, 0xc0, 0xfc, 0x00,
            0xfc, 0xc0, 0xc0, 0xf8, 0xc0, 0xc0, 0xc0, 0x00,
            0x78, 0xcc, 0xc0, 0xdc, 0xcc, 0xcc, 0x78, 0x00,
            0xcc, 0xcc, 0xcc, 0xfc, 0xcc, 0xcc, 0xcc, 0x00,
            0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0x00,
            0x18, 0x18, 0x18, 0x18, 0x18, 0xd8, 0x70, 0x00,
            0xcc, 0xd8, 0xf0, 0xe0, 0xf0, 0xd8, 0xcc, 0x00,
            0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xfc, 0x00,
            0xc6, 0xee, 0xfe, 0xd6, 0xc6, 0xc6, 0xc6, 0x00,
            0xcc, 0xec, 0xfc, 0xfc, 0xdc, 0xcc, 0xcc, 0x00,
            0x78, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0x78, 0x00,
            0xf8, 0xcc, 0xcc, 0xf8, 0xc0, 0xc0, 0xc0, 0x00,
            0x78, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0x78, 0x0c,
            0xf8, 0xcc, 0xcc, 0xf8, 0xf0, 0xd8, 0xcc, 0x00,
            0x78, 0xcc, 0xc0, 0x78, 0x0c, 0xcc, 0x78, 0x00,
            0xfc, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00,
            0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0x78, 0x00,
            0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0x78, 0x30, 0x00,
            0xc6, 0xc6, 0xc6, 0xd6, 0xfe, 0xee, 0xc6, 0x00,
            0xcc, 0xcc, 0x78, 0x30, 0x78, 0xcc, 0xcc, 0x00,
            0xcc, 0xcc, 0xcc, 0x78, 0x30, 0x30, 0x30, 0x00,
            0xfc, 0x0c, 0x18, 0x30, 0x60, 0xc0, 0xfc, 0x00,
            0xcc, 0x00, 0xcc, 0xcc, 0xcc, 0xcc, 0x7c, 0x00,
            0xcc, 0x00, 0x78, 0x0c, 0x7c, 0xcc, 0x7c, 0x00,
            0xe8, 0x4d, 0x4a, 0x48, 0x00, 0x00, 0x00, 0x00,
            0x80, 0x80, 0x80, 0x80, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xfe,
            0xfe, 0x7c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x78, 0x0c, 0x7c, 0xcc, 0x7c, 0x00,
            0xc0, 0xc0, 0xf8, 0xcc, 0xcc, 0xcc, 0xf8, 0x00,
            0x00, 0x00, 0x78, 0xc0, 0xc0, 0xc0, 0x78, 0x00,
            0x0c, 0x0c, 0x7c, 0xcc, 0xcc, 0xcc, 0x7c, 0x00,
            0x00, 0x00, 0x78, 0xcc, 0xfc, 0xc0, 0x78, 0x00,
            0x38, 0x60, 0xf8, 0x60, 0x60, 0x60, 0x60, 0x00,
            0x00, 0x00, 0x78, 0xcc, 0xcc, 0x7c, 0x0c, 0x78,
            0xc0, 0xc0, 0xf8, 0xcc, 0xcc, 0xcc, 0xcc, 0x00,
            0xc0, 0x00, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0x00,
            0x30, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0xe0,
            0xc0, 0xc0, 0xcc, 0xd8, 0xf0, 0xd8, 0xcc, 0x00,
            0xe0, 0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x00,
            0x00, 0x00, 0xcc, 0xfe, 0xfe, 0xd6, 0xc6, 0x00,
            0x00, 0x00, 0xf8, 0xcc, 0xcc, 0xcc, 0xcc, 0x00,
            0x00, 0x00, 0x78, 0xcc, 0xcc, 0xcc, 0x78, 0x00,
            0x00, 0x00, 0xf8, 0xcc, 0xcc, 0xf8, 0xc0, 0xc0,
            0x00, 0x00, 0x7c, 0xcc, 0xcc, 0x7c, 0x0c, 0x0c,
            0x00, 0x00, 0xf8, 0xcc, 0xc0, 0xc0, 0xc0, 0x00,
            0x00, 0x00, 0x7c, 0xc0, 0x78, 0x0c, 0xf8, 0x00,
            0x30, 0x30, 0xfc, 0x30, 0x30, 0x30, 0x1c, 0x00,
            0x00, 0x00, 0xcc, 0xcc, 0xcc, 0xcc, 0x7c, 0x00,
            0x00, 0x00, 0xcc, 0xcc, 0xcc, 0x78, 0x30, 0x00,
            0x00, 0x00, 0xc6, 0xd6, 0xfe, 0x7c, 0x6c, 0x00,
            0x00, 0x00, 0xcc, 0x78, 0x30, 0x78, 0xcc, 0x00,
            0x00, 0x00, 0xcc, 0xcc, 0xcc, 0x7c, 0x18, 0x70,
            0x00, 0x00, 0xfc, 0x18, 0x30, 0x60, 0xfc, 0x00,
            0xcc, 0x00, 0x78, 0xcc, 0xcc, 0xcc, 0x78, 0x00,
            0xcc, 0x30, 0x78, 0xcc, 0xfc, 0xcc, 0xcc, 0x00,
            0x84, 0x78, 0xcc, 0xcc, 0xcc, 0xcc, 0x78, 0x00,
            0x38, 0x6c, 0x6c, 0xf8, 0xcc, 0xcc, 0xf8, 0x80,
            0x08, 0x0c, 0x0e, 0xff, 0xff, 0x0e, 0x0c, 0x08,
            0x30, 0x78, 0xcc, 0xfc, 0xcc, 0xcc, 0xcc, 0x00,
            0xf8, 0xc0, 0xc0, 0xf8, 0xcc, 0xcc, 0xf8, 0x00,
            0xf8, 0xcc, 0xcc, 0xf8, 0xcc, 0xcc, 0xf8, 0x00,
            0xfc, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0x00,
            0x3c, 0x6c, 0x6c, 0x6c, 0x6c, 0x6c, 0xfe, 0xc6,
            0xfc, 0xc0, 0xc0, 0xf8, 0xc0, 0xc0, 0xfc, 0x00,
            0xd6, 0xd6, 0x7c, 0xd6, 0xd6, 0xd6, 0xd6, 0x00,
            0x78, 0x8c, 0x0c, 0x38, 0x0c, 0x8c, 0x78, 0x00,
            0xcc, 0xcc, 0xdc, 0xfc, 0xfc, 0xec, 0xcc, 0x00,
            0x30, 0xcc, 0xdc, 0xfc, 0xfc, 0xec, 0xcc, 0x00,
            0xcc, 0xd8, 0xf0, 0xe0, 0xf0, 0xd8, 0xcc, 0x00,
            0x3c, 0x6c, 0x6c, 0x6c, 0x6c, 0x6c, 0xcc, 0x00,
            0xc6, 0xee, 0xfe, 0xd6, 0xc6, 0xc6, 0xc6, 0x00,
            0xcc, 0xcc, 0xcc, 0xfc, 0xcc, 0xcc, 0xcc, 0x00,
            0x78, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0x78, 0x00,
            0xfc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0x00,
            0xf8, 0xcc, 0xcc, 0xf8, 0xc0, 0xc0, 0xc0, 0x00,
            0x78, 0xcc, 0xc0, 0xc0, 0xc0, 0xcc, 0x78, 0x00,
            0xfc, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00,
            0xcc, 0xcc, 0xcc, 0x7c, 0x0c, 0xcc, 0x78, 0x00,
            0x78, 0xb4, 0xb4, 0xb4, 0x78, 0x30, 0x30, 0x00,
            0xcc, 0xcc, 0x78, 0x30, 0x78, 0xcc, 0xcc, 0x00,
            0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xfe, 0x06,
            0xcc, 0xcc, 0xcc, 0x7c, 0x0c, 0x0c, 0x0c, 0x00,
            0xd6, 0xd6, 0xd6, 0xd6, 0xd6, 0xd6, 0xfe, 0x00,
            0xd6, 0xd6, 0xd6, 0xd6, 0xd6, 0xd6, 0xfe, 0x06,
            0xe0, 0x60, 0x60, 0x7c, 0x66, 0x66, 0x7c, 0x00,
            0xc6, 0xc6, 0xc6, 0xf6, 0xde, 0xde, 0xf6, 0x00,
            0xc0, 0xc0, 0xc0, 0xf8, 0xcc, 0xcc, 0xf8, 0x00,
            0x78, 0x8c, 0x0c, 0x3c, 0x0c, 0x8c, 0x78, 0x00,
            0xcc, 0xd6, 0xd6, 0xf6, 0xd6, 0xd6, 0xcc, 0x00,
            0x7c, 0xcc, 0xcc, 0x7c, 0x3c, 0x6c, 0xcc, 0x00,
            0x00, 0x00, 0x78, 0x0c, 0x7c, 0xcc, 0x7c, 0x00,
            0x04, 0x78, 0xc0, 0xf8, 0xcc, 0xcc, 0x78, 0x00,
            0x00, 0x00, 0xf8, 0xcc, 0xf8, 0xcc, 0xf8, 0x00,
            0x00, 0x00, 0xf0, 0xc0, 0xc0, 0xc0, 0xc0, 0x00,
            0x00, 0x00, 0x3c, 0x6c, 0x6c, 0x6c, 0xfe, 0xc6,
            0x00, 0x00, 0x78, 0xcc, 0xfc, 0xc0, 0x78, 0x00,
            0x00, 0x00, 0xd6, 0x7c, 0xd6, 0xd6, 0xd6, 0x00,
            0x00, 0x00, 0x78, 0x9c, 0x38, 0x9c, 0x78, 0x00,
            0x00, 0x00, 0xcc, 0xdc, 0xfc, 0xec, 0xcc, 0x00,
            0x10, 0x20, 0xcc, 0xdc, 0xfc, 0xec, 0xcc, 0x00,
            0x00, 0x00, 0xcc, 0xd8, 0xf0, 0xd8, 0xcc, 0x00,
            0x00, 0x00, 0x3c, 0x6c, 0x6c, 0x6c, 0xcc, 0x00,
            0x00, 0x00, 0xc6, 0xee, 0xfe, 0xd6, 0xc6, 0x00,
            0x00, 0x00, 0xcc, 0xcc, 0xfc, 0xcc, 0xcc, 0x00,
            0x00, 0x00, 0x78, 0xcc, 0xcc, 0xcc, 0x78, 0x00,
            0x00, 0x00, 0xfc, 0xcc, 0xcc, 0xcc, 0xcc, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0xf8, 0xcc, 0xcc, 0xf8, 0xc0, 0xc0,
            0x00, 0x00, 0x78, 0xc0, 0xc0, 0xc0, 0x78, 0x00,
            0x00, 0x00, 0xfc, 0x30, 0x30, 0x30, 0x30, 0x00,
            0x00, 0x00, 0xcc, 0xcc, 0xcc, 0x7c, 0x18, 0x70,
            0x00, 0x00, 0x78, 0xb4, 0xb4, 0x78, 0x30, 0x30,
            0x00, 0x00, 0xcc, 0x78, 0x30, 0x78, 0xcc, 0x00,
            0x00, 0x00, 0xd8, 0xd8, 0xd8, 0xd8, 0xfc, 0x0c,
            0x00, 0x00, 0xd8, 0xd8, 0x78, 0x18, 0x18, 0x00,
            0x00, 0x00, 0xd6, 0xd6, 0xd6, 0xd6, 0xfe, 0x00,
            0x00, 0x00, 0xd6, 0xd6, 0xd6, 0xd6, 0xfe, 0x06,
            0x00, 0x00, 0xe0, 0x60, 0x7c, 0x66, 0x7c, 0x00,
            0x00, 0x00, 0xc6, 0xc6, 0xf6, 0xce, 0xf6, 0x00,
            0x00, 0x00, 0xc0, 0xc0, 0xf8, 0xcc, 0xf8, 0x00,
            0x00, 0x00, 0x78, 0x8c, 0x3c, 0x8c, 0x78, 0x00,
            0x00, 0x00, 0xcc, 0xd6, 0xf6, 0xd6, 0xcc, 0x00,
            0x00, 0x00, 0x7c, 0xcc, 0x7c, 0x3c, 0xec, 0x00,
            0x48, 0xfc, 0xc0, 0xf8, 0xc0, 0xc0, 0xfc, 0x00,
            0x48, 0x00, 0x78, 0xcc, 0xfc, 0xc0, 0x78, 0x00
        };

        // These replacement character tables have been created using the
        // make-charset-fontdata tool.

        // German Zak font.
        static readonly byte[] germanCharsetDataV2 =
        {
            36, 0,
            37, 1,
            39, 2,
            61, 3,
            91, 4,
            92, 5,
            93, 6,
            123, 7,
            124, 8,
            125, 9,
            126, 10,
        };

        // German v0 MM.
        static readonly byte[] germanCharsetDataV0 =
        {
            36, 11,
            42, 12,
            43, 13,
            60, 4,
            62, 7,
            91, 5,
            93, 14,
            94, 15,
        };

        // French Zak font.
        static readonly byte[] frenchCharsetDataV2 =
        {
            36, 0,
            37, 1,
            39, 2,
            60, 16,
            61, 17,
            62, 18,
            91, 19,
            92, 20,
            93, 21,
            95, 22,
            123, 23,
            124, 24,
            125, 25,
            126, 26,
        };

        // Italian Zak font.
        static readonly byte[] italianCharsetDataV2 =
        {
            36, 0,
            37, 1,
            39, 2,
            60, 16,
            61, 27,
            62, 20,
            91, 19,
            92, 20,
            93, 21,
            95, 22,
            123, 28,
            124, 29,
            125, 25,
            126, 26,
        };

        // Spanish Zak font.
        static readonly byte[] spanishCharsetDataV2 =
        {
            36, 0,
            37, 1,
            39, 2,
            61, 3,
            91, 4,
            92, 5,
            93, 6,
            123, 7,
            124, 8,
            125, 9,
            126, 10,
        };

        // Special characters
        static readonly byte[] specialCharsetData = {
            0x18, 0x3e, 0x60, 0x3c, 0x06, 0x7c, 0x18, 0x00,
            0x62, 0x66, 0x0c, 0x18, 0x30, 0x66, 0x46, 0x00,
            0x30, 0x18, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x7c, 0x82, 0xba, 0xa2, 0xa2, 0xba, 0x82, 0x7c,
            0x66, 0x00, 0x66, 0x66, 0x66, 0x66, 0x3e, 0x00,
            0x66, 0x00, 0x3c, 0x06, 0x3e, 0x66, 0x3e, 0x00,
            0x66, 0x00, 0x66, 0x66, 0x66, 0x66, 0x3c, 0x00,
            0x66, 0x00, 0x3c, 0x66, 0x66, 0x66, 0x3c, 0x00,
            0x66, 0x18, 0x3c, 0x66, 0x7e, 0x66, 0x66, 0x00,
            0x42, 0x3c, 0x66, 0x66, 0x66, 0x66, 0x3c, 0x00,
            0x1c, 0x36, 0x36, 0x7c, 0x66, 0x66, 0x7c, 0x40,
            0x66, 0x3c, 0x66, 0x66, 0x66, 0x66, 0x3c, 0x00,
            0x3c, 0x66, 0x66, 0x7c, 0x66, 0x66, 0x6c, 0x60,
            0x66, 0x3c, 0x66, 0x7e, 0x66, 0x66, 0x66, 0x00,
            0x66, 0x24, 0x66, 0x66, 0x66, 0x66, 0x3c, 0x00,
            0x00, 0x18, 0x3c, 0x7e, 0x18, 0x18, 0x18, 0x18,
            0x10, 0x08, 0x3c, 0x06, 0x3e, 0x66, 0x3e, 0x00,
            0x18, 0x24, 0x3c, 0x06, 0x3e, 0x66, 0x3e, 0x00,
            0x00, 0x00, 0x3c, 0x60, 0x60, 0x3c, 0x18, 0x38,
            0x08, 0x10, 0x3c, 0x66, 0x7e, 0x60, 0x3c, 0x00,
            0x10, 0x08, 0x3c, 0x66, 0x7e, 0x60, 0x3c, 0x00,
            0x18, 0x24, 0x3c, 0x66, 0x7e, 0x60, 0x3c, 0x00,
            0x00, 0x6c, 0x00, 0x38, 0x18, 0x18, 0x3c, 0x00,
            0x18, 0x24, 0x00, 0x38, 0x18, 0x18, 0x3c, 0x00,
            0x18, 0x24, 0x00, 0x3c, 0x66, 0x66, 0x3c, 0x00,
            0x10, 0x08, 0x00, 0x66, 0x66, 0x66, 0x3e, 0x00,
            0x18, 0x24, 0x00, 0x66, 0x66, 0x66, 0x3e, 0x00,
            0x08, 0x10, 0x3c, 0x06, 0x3e, 0x66, 0x3e, 0x00,
            0x10, 0x08, 0x00, 0x38, 0x18, 0x18, 0x3c, 0x00,
            0x10, 0x08, 0x00, 0x3c, 0x66, 0x66, 0x3c, 0x00,
        };
    }
    
}
