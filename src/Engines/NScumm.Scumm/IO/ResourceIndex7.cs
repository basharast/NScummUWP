//
//  ResourceIndex7.cs
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

using System.Collections.ObjectModel;
using System.IO;
using NScumm.Core;

namespace NScumm.Scumm.IO
{
    class ResourceIndex7 : ResourceIndex6
    {
        public override int NumVerbs { get { return numVerbs; } }

        public override int NumInventory { get { return numInventory; } }

        public override int NumVariables { get { return numVariables; } }

        public override int NumBitVariables { get { return numBitVariables; } }

        public override int NumLocalObjects { get { return numLocalObjects; } }

        public override int NumArray { get { return numArray; } }

        public override int NumGlobalScripts { get { return numGlobalScripts; }}

        public override byte[] ObjectRoomTable { get { return objectRoomTable; } }

        protected override void LoadIndex(GameInfo game)
        {
            Directory = ServiceLocator.FileStorage.GetDirectoryName(game.Path);
            using (var file = ServiceLocator.FileStorage.OpenFileRead(game.Path))
            {
                var br = new BinaryReader(file);
                
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var tag = System.Text.Encoding.UTF8.GetString(br.ReadBytes(4));
                    br.ReadUInt32BigEndian();

                    switch (tag)
                    {
                        case "DCHR":
                        case "DIRF":
                            var charset = ReadResTypeList(br);
                            CharsetResources = new ReadOnlyCollection<Resource>(charset);
                            break;

                        case "DOBJ":
                            ReadDirectoryOfObjects(br);
                            break;

                        case "RNAM":
                            ReadRoomNames(br);
                            break;

                        case "DROO":
                        case "DIRR":
                            var rooms = ReadResTypeList(br);
                            RoomResources = new ReadOnlyCollection<Resource>(rooms);
                            break;
                        case "DSCR":
                        case "DIRS":
                            var scripts = ReadResTypeList(br);
                            ScriptResources = new ReadOnlyCollection<Resource>(scripts);
                            break;

                        case "DCOS":
                        case "DIRC":
                            var costumes = ReadResTypeList(br);
                            CostumeResources = new ReadOnlyCollection<Resource>(costumes);
                            break;

                        case "MAXS":
                            ReadMaxSizes(br);
                            break;

                        case "DIRN":
                        case "DSOU":
                            var sounds = ReadResTypeList(br);
                            SoundResources = new ReadOnlyCollection<Resource>(sounds);
                            break;

                        case "AARY":
                            ReadArrayFromIndexFile(br);
                            break;
                        
                        case "ANAM":        // Used by: The Dig, FT
                            {
                                var num = br.ReadUInt16();
                                AudioNames = new string[num];
                                for (int i = 0; i < num; i++)
                                {
									AudioNames[i] = br.ReadBytes(9).GetText();
                                }
                            }
                            break;

//                        default:
//                            Console.Error.WriteLine("Unknown tag {0} found in index file directory", tag);
//                            break;
                    }
                }
            }
        }

        protected override void ReadMaxSizes(BinaryReader reader) 
        {
            reader.BaseStream.Seek(50, SeekOrigin.Current);  // Skip over SCUMM engine version
            reader.BaseStream.Seek(50, SeekOrigin.Current);  // Skip over data file version
            numVariables = reader.ReadUInt16();
            numBitVariables = reader.ReadUInt16();
            reader.ReadUInt16();
            var numGlobalObjects = reader.ReadUInt16();
            numLocalObjects = reader.ReadUInt16();
            var numNewNames = reader.ReadUInt16();
            numVerbs = reader.ReadUInt16();
            var numFlObject = reader.ReadUInt16();
            numInventory = reader.ReadUInt16();
            numArray = reader.ReadUInt16();
            var numRooms = reader.ReadUInt16();
            var numScripts = reader.ReadUInt16();
            var numSounds = reader.ReadUInt16();
            var numCharsets = reader.ReadUInt16();
            var numCostumes = reader.ReadUInt16();

//            _objectRoomTable = (byte *)calloc(_numGlobalObjects, 1);
//
            if ((Game.GameId == GameId.FullThrottle) && (Game.Features.HasFlag(GameFeatures.Demo)) /*&& (_game.platform == Common::kPlatformDOS)*/)
                numGlobalScripts = 300;
            else
                numGlobalScripts = 2000;
//
//            _shadowPaletteSize = NUM_SHADOW_PALETTE * 256;
//            _shadowPalette = (byte *)calloc(_shadowPaletteSize, 1);
        }

        protected override void ReadDirectoryOfObjects(BinaryReader br)
        {
            int num = br.ReadUInt16();

            ObjectStateTable = br.ReadBytes(num);
            objectRoomTable = br.ReadBytes(num);

            ObjectOwnerTable = new byte[num];
            for (int i = 0; i < num; i++)
            {
                ObjectOwnerTable[i] = 0xFF;
            }

            ClassData = br.ReadUInt32s(num);

            #if SCUMM_BIG_ENDIAN
            // Correct the endianess if necessary
            for (int i = 0; i != num; i++)
            _classData[i] = FROM_LE_32(_classData[i]);
            #endif
        }

        protected int numVerbs;
        protected int numInventory;
        protected int numVariables;
        protected int numBitVariables;
        protected int numLocalObjects;
        protected int numGlobalScripts;
        protected int numArray;
        protected byte[] objectRoomTable;
    }
}
