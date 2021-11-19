﻿//
//  FileStorage.cs
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
using System.IO;
using System.Reflection;
using NScumm.Core;
using System.Text;
using System.Xml.Linq;

namespace NScumm
{
    public class FileStorage: IFileStorage
    {
        public System.Collections.Generic.IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        public System.Collections.Generic.IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern);
        }

        public System.Collections.Generic.IEnumerable<string> EnumerateFiles(string path, string searchPattern, NScumm.Core.SearchOption option)
        {
            System.IO.SearchOption sysOption = System.IO.SearchOption.TopDirectoryOnly;
            switch (option)
            {
                case NScumm.Core.SearchOption.TopDirectoryOnly:
                    sysOption = System.IO.SearchOption.TopDirectoryOnly;
                    break;
                case NScumm.Core.SearchOption.AllDirectories:
                    sysOption = System.IO.SearchOption.AllDirectories;
                    break;
            }
            return Directory.EnumerateFiles(path, searchPattern, sysOption);
        }

        public string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public string ChangeExtension(string path, string newExtension)
        {
            return Path.ChangeExtension(path, newExtension);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public Stream OpenFileRead(string path)
        {
            return File.OpenRead(path);
        }

        public Stream OpenFileWrite(string path)
        {
            return File.OpenWrite(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string GetSignature(string path)
        {
            string signature;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {

                using (var file = File.OpenRead(path))
                {
                    var br = new BinaryReader(file);
                    var data = br.ReadBytes(1024 * 1024);
                    var md5Key = md5.ComputeHash(data, 0, data.Length);
                    var md5Text = new StringBuilder();
                    for (int i = 0; i < 16; i++)
                    {
                        md5Text.AppendFormat("{0:x2}", md5Key[i]);
                    }
                    signature = md5Text.ToString();
                }
            }
            return signature;
        }

        public XDocument LoadDocument(Stream stream)
        {
            return XDocument.Load(stream);
        }

        public Stream OpenContent(string path)
        {
			var exe=Environment.GetCommandLineArgs ()[0];
            var dir = Path.GetDirectoryName(exe);
            var fullPath = ScummHelper.LocatePath(dir, path);
            return OpenFileRead(fullPath);
        }
    }
}

