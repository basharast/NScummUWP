﻿//  Author:
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

using System.Globalization;
using NScumm.Core.Graphics;

namespace NScumm.Core.IO
{
    public interface IGameDescriptor
    {
        string Id { get; }
        string Description { get; }
        CultureInfo Culture { get; }
        Platform Platform { get; }
        int Width { get; }
        int Height { get; }

        int Version { get; set; }

        PixelFormat PixelFormat { get; }
        string Path { get; }
    }
}