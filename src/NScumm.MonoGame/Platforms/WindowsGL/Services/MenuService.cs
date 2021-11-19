﻿//
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
using System.Linq;

namespace NScumm.MonoGame.Services
{
    class MenuService : IMenuService
    {
        private ScummGame _game;

        public MenuService(ScummGame game)
        {
            _game = game;
        }

        public void ShowMenu()
        {
            var scummScreen = _game.ScreenManager.GetScreens().OfType<ScummScreen>().FirstOrDefault();
            _game.ScreenManager.AddScreen(new MainMenuScreen(scummScreen));
        }
    }
}
