//
//  RateHelper.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2014 
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


namespace NScumm.Core.Audio
{
    public static class RateHelper
    {
        public const int IntermediateBufferSize = 512;
        public const int SampleMax = short.MaxValue;
        public const int SampleMin = short.MinValue;

        public static void ClampedAdd(ref short a, int b)
        {
            int val = a + b;
            
            if (val > SampleMax)
                val = SampleMax;
            else if (val < SampleMin)
                val = SampleMin;

            a = (short)val;
        }

        public static IRateConverter MakeRateConverter(int inrate, int outrate, bool stereo, bool reverseStereo)
        {
            if (inrate != outrate)
            {
                if ((inrate % outrate) == 0)
                {
                    return new SimpleRateConverter(inrate, outrate, stereo, reverseStereo);
                }
                else
                {
                    return new LinearRateConverter(inrate, outrate, stereo, reverseStereo);
                }
            }
            else
            {
                return new CopyRateConverter(stereo, reverseStereo);
            }
        }
    }
}
