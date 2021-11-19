//
//  ResourceFile4.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2014 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NScumm.Core.Graphics;
using System.Linq;
using NScumm.Core;
using NScumm.Core.Audio;
using NScumm.Scumm.Graphics;

namespace NScumm.Scumm.IO
{
    class ResourceFile5: ResourceFile4
    {
        public ResourceFile5(Stream stream)
            : base(stream)
        {

        }

        protected IEnumerator<Chunk> CreateChunkIterator(long size)
        {
            return new ChunkIterator5(_reader, size);
        }

        protected override void GotoResourceHeader(long offset)
        {
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        protected static string ToTag(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public override Room ReadRoom(long offset)
        {
            var its = new Stack<IEnumerator<Chunk>>();
            var room = new Room();
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var it = CreateChunkIterator(_reader.BaseStream.Length - offset);
            var images = new Dictionary<ushort, ObjectData>();
            do
            {
                while (it.MoveNext())
                {
                    switch (it.Current.Tag)
                    {
                        case "LFLF":
                                    // disk block
                            room.Number = _reader.ReadUInt16();
                            it = CreateChunkIterator(it.Current.Size - 2);
                            break;
                        
                        case "ROOM":
                                    // room block
                            its.Push(it);
                            it = CreateChunkIterator(it.Current.Size);
                            break;

                        case "TRNS":
                            {
                                // transparent color
                                room.TransparentColor = _reader.ReadByte();
                                var unknown = _reader.ReadByte();
                            }
                            break;

                        case "RMHD":
                                    // ROOM Header
                            room.Header = ReadRMHD();
                            break;

                        case "RMIM":
                                    // room image
                            its.Push(it);
                            it = CreateChunkIterator(it.Current.Size);
                            break;
                                
                        case "RMIH":
                            // number of z buffers
                            room.NumZBuffer = _reader.ReadUInt16() + 1;
                            break;

                        case "CYCL":
                                    // CYCL
                            room.ColorCycle = ReadCYCL();
                            break;
                        case "EPAL":
                                    // EPAL
                            ReadEPAL();
                            break;
                        case "BOXD":
                                    // box data
                            {
                                var numBoxes = _reader.ReadUInt16();
                                for (int i = 0; i < numBoxes; i++)
                                {
                                    var box = ReadBox();
                                    room.Boxes.Add(box);
                                }
                            }
                            break;
                        case "BOXM":
                                    // box matrix
                            {
                                var size = (int)(it.Current.Size - 8);
                                room.BoxMatrix.Clear();
                                room.BoxMatrix.AddRange(_reader.ReadBytes(size));
                            }
                            break;
                        case "CLUT":
                            {
                                // CLUT
                                var colors = ReadCLUT();
                                room.HasPalette = true;
                                Array.Copy(colors, room.Palette.Colors, colors.Length);
                            }
                            break;
                        case "SCAL":
                                    // SCAL
                            if (it.Current.Size > 8)
                            {
                                room.Scales = ReadSCAL();
                            }
                            break;
                        case "ENCD":
                            {
                                // Entry script
                                room.EntryScript.Data = _reader.ReadBytes((int)(it.Current.Size - 8));
                            }
                            break;
                        case "EXCD":
                            {
                                // exit script
                                room.ExitScript.Data = _reader.ReadBytes((int)(it.Current.Size - 8));
                            }
                            break;
                        case "NLSC":
                            {
                                // number of local scripts
                                var numLocalScripts = _reader.ReadUInt16();
                            }
                            break;
                        case "LSCR":
                            {
                                // local script
                                var index = _reader.ReadByte();
                                room.LocalScripts[index - 200] = new ScriptData
                                {
                                    Data = _reader.ReadBytes((int)(it.Current.Size - 9))
                                };
                            }
                            break;
                        case "OBIM":
                            {
                                // Object Image
                                var obj = ReadObjectImages(it.Current.Size - 8);
                                images[obj.Number] = obj;
                            }
                            break;
                        case "OBCD":
                            {
                                // object script
                                var obj = ReadObjectCode(it.Current.Size - 8);
                                room.Objects.Add(Merge(images[obj.Number], obj));
                            }
                            break;                        

                        default:
                            if (it.Current.Tag.StartsWith("IM", StringComparison.Ordinal))
                            {
                                var smapNum = int.Parse(it.Current.Tag.Substring(2), System.Globalization.NumberStyles.HexNumber);
                                var img = ReadImage(it.Current.Size - 8, room.Header.Width / 8);
                                room.Image = img;
                            }
                            else
                            {
                                UnknownChunk(it.Current);
                            }
                            break;
                    }
                }
                it = its.Pop();
            } while (its.Count > 0);

            return room;
        }

        protected virtual ObjectData Merge(ObjectData objImg, ObjectData objCode)
        {
            objCode.Hotspots.AddRange(objImg.Hotspots);
            objCode.Images.AddRange(objImg.Images);
            return objCode;
        }

        protected ObjectData ReadObjectImages(long size)
        {
            ObjectData od = null;
            var it = CreateChunkIterator(size);
            while (it.MoveNext())
            {
                switch (it.Current.Tag)
                {
                    case "IMHD":
                        {
                            od = ReadImageHeader();
                        }
                        break;
                    case "IMAG":
                        {
                            od.Images.AddRange(ReadImages(it.Current.Size));
                        }
                        break;
                    default:
                        var img = ReadImage(it.Current.Size - 8, od.Width / 8);
                        od.Images.Add(img);
                        break;
                }
            }
            return od;
        }

        protected virtual ObjectData ReadImageHeader()
        {
            var od = new ObjectData(_reader.ReadUInt16());
            var numImnn = _reader.ReadUInt16();
            var numZpnn = _reader.ReadUInt16();
            od.Flags = (DrawBitmaps)_reader.ReadByte();
            var unknown1 = _reader.ReadByte();
            od.Position = new Point(_reader.ReadInt16(), _reader.ReadInt16());
            od.Width = _reader.ReadUInt16();
            od.Height = _reader.ReadUInt16();
            return od;
        }

        protected virtual ObjectData ReadCDHD()
        {
            var obj = new ObjectData(_reader.ReadUInt16())
            {
                Position = new Point((short)(_reader.ReadByte() * 8), (short)(_reader.ReadByte() * 8)),
                Width = (ushort)(_reader.ReadByte() * 8),
                Height = (ushort)(_reader.ReadByte() * 8),
                Flags = (DrawBitmaps)_reader.ReadByte(),
                Parent = _reader.ReadByte(),
                Walk = new Point(_reader.ReadInt16(), _reader.ReadInt16()),
                ActorDir = _reader.ReadByte()
            };
            obj.ParentState = (obj.Flags == (DrawBitmaps)0x80) ? (byte)1 : (byte)((int)obj.Flags & 0xF);
            return obj;
        }

        protected ObjectData ReadObjectCode(long size)
        {
            ObjectData obj = null;
            var it = CreateChunkIterator(size);
            while (it.MoveNext())
            {
                switch (it.Current.Tag)
                {
                    case "CDHD":
                        {
                            // code header
                            obj = ReadCDHD();
                        }
                        break;
                    case "VERB":
                        {
                            ReadVerbTable(obj, (int)(it.Current.Size - 8));
                            obj.Script.Data = ReadVerbScript(obj, it.Current);
                        }
                        break;
                    case "OBNA":
                        {
                            obj.Name = ReadObjectName();
                        }
                        break;
                    default:
                        UnknownChunk(it.Current);
                        break;
                }
            }
            return obj;
        }

        protected virtual byte[] ReadVerbScript(ObjectData obj, Chunk chunk)
        {
            return _reader.ReadBytes((int)(chunk.Size - obj.Script.Offset));
        }

        protected override void ReadVerbTable(ObjectData data, int size)
        {
            var pos = _reader.BaseStream.Position - 8;
            byte id;
            while ((id = _reader.ReadByte()) != 0)
            {
                var offset = _reader.ReadUInt16();
                if (!data.ScriptOffsets.ContainsKey(id))
                {
                    data.ScriptOffsets[id] = offset;
                }
            }
            var realCodeOffset = _reader.BaseStream.Position - pos;
            var virtualCodeOffset = (3 * data.ScriptOffsets.Count + 1 + 8);
            var diff = (ushort)(realCodeOffset - virtualCodeOffset);
            foreach (var key in data.ScriptOffsets.Keys.ToList())
            {
                data.ScriptOffsets[key] -= diff;
            }
            data.Script.Offset = virtualCodeOffset;
        }

        protected override Color[] ReadCLUT()
        {
            var colors = new Color[256];
            for (var i = 0; i < 256; i++)
            {
                colors[i] = Color.FromRgb(_reader.ReadByte(), _reader.ReadByte(), _reader.ReadByte());
            }
            return colors;
        }

        protected virtual ImageData ReadBomp(long size)
        {
            var id = _reader.ReadUInt16();
            var width = _reader.ReadUInt16();
            var height = _reader.ReadUInt16();
            var padding = _reader.ReadUInt32();
            var img = new ImageData();
            img.Data = _reader.ReadBytes((int)(size - 10));
            img.IsBomp = true;
            return img;
        }

        protected IList<ImageData> ReadImages(long size)
        {
            var images = new List<ImageData>();
            var it = CreateChunkIterator(size);
            while (it.MoveNext())
            {
                if (it.Current.Tag == "WRAP")
                {
                    it = CreateChunkIterator(it.Current.Size);
                }
                else if (it.Current.Tag == "OFFS")
                {
                    // skip it
                }
                else if (it.Current.Tag == "BOMP")
                {
                    var img = ReadBomp(it.Current.Size - 8);
                    images.Add(img);
                }
                else if (it.Current.Tag == "OBIM")
                {
                    // Object Image
                    var obj = ReadObjectImages(it.Current.Size - 8);
                    images.AddRange(obj.Images);
                }
                else if (it.Current.Tag == "SMAP")
                {
                    var img = new ImageData();
                    img.Data = _reader.ReadBytes((int)(it.Current.Size - 8));
                    images.Add(img);
                }
                else
                {
                    UnknownChunk(it.Current);
                }
            }
            return images;
        }

        protected ImageData ReadImage(long size, int numStrips)
        {
            var img = new ImageData();
            var it = CreateChunkIterator(size);
            while (it.MoveNext())
            {
                if (it.Current.Tag == "SMAP")
                {
                    img.Data = _reader.ReadBytes((int)(it.Current.Size));
                }
                else if (it.Current.Tag.StartsWith("ZP", StringComparison.Ordinal))
                {
                    var zpNum = int.Parse(it.Current.Tag.Substring(2), System.Globalization.NumberStyles.HexNumber);
                    ZPlane zplane;
                    using (var ms = new MemoryStream(_reader.ReadBytes((int)(it.Current.Size))))
                    {
                        var br = new BinaryReader(ms);
                        zplane = ReadZPlane(br, (int)(it.Current.Size), numStrips);
                    }
                    img.ZPlanes.Add(zplane);
                }
                else if (it.Current.Tag == "BOMP")
                {
                    img = ReadBomp(it.Current.Size - 8);
                }
                else
                {
                    UnknownChunk(it.Current);
                }
            }
            return img;
        }

        protected override ZPlane ReadZPlane(BinaryReader b, int size, int numStrips)
        {
            var zPlaneData = b.ReadBytes(size);
            byte[] strips = null;
            var offsets = new List<int?>();
            using (var ms = new MemoryStream(zPlaneData))
            {
                var br = new BinaryReader(ms);
                var tableSize = 8 + numStrips * 2;

                // read table offsets
                for (int i = 0; i < numStrips; i++)
                {
                    var offset = br.ReadUInt16();
                    if (offset != 0)
                    {
                        offsets.Add(offset - tableSize);
                    }
                    else
                    {
                        offsets.Add(null);
                    }
                }
                strips = br.ReadBytes(size - tableSize);
            }
            var zPlane = new ZPlane(strips, offsets);
            return zPlane;
        }

        protected static void UnknownChunk(Chunk chunk)
        {
            System.Diagnostics.Debug.WriteLine("Ignoring Resource Tag: {0}, Size: {1:X4}", chunk.Tag, chunk.Size);
        }

        protected byte[] ReadObjectName()
        {
            byte chr;
            var name = new List<byte>();
            while ((chr = _reader.ReadByte()) != 0)
            {
                name.Add(chr);
            }
            return name.ToArray();
        }

        public override byte[] ReadSound(MusicDriverTypes music, long offset)
        {
            GotoResourceHeader(offset);
            var tag = ToTag(_reader.ReadBytes(4));
            if (tag != "SOUN")
                throw new NotSupportedException("Expected SO block.");
            var size = _reader.ReadUInt32BigEndian();
            var pos = _reader.BaseStream.Position + size - 8;

            do
            {
                tag = ToTag(_reader.ReadBytes(4));
                size = _reader.ReadUInt32BigEndian();

                switch (tag)
                {
                    case "ADL ":
                        if (music == MusicDriverTypes.AdLib|| music == MusicDriverTypes.FMTowns)
                        {
                            _reader.BaseStream.Seek(-8, SeekOrigin.Current);
                            return _reader.ReadBytes((int)size + 8);
                        }
                        _reader.BaseStream.Seek(size, SeekOrigin.Current);
                        break;
                    case "AMI ":
                        _reader.BaseStream.Seek(-8, SeekOrigin.Current);
                        return _reader.ReadBytes((int)size + 8);
                    case "SPK ":
                        if (music == MusicDriverTypes.PCSpeaker)
                        {
                            _reader.BaseStream.Seek(-8, SeekOrigin.Current);
                            return _reader.ReadBytes((int)size + 8);
                        }
                        _reader.BaseStream.Seek(size, SeekOrigin.Current);
                        break;
                    case "MIDI":
                        if (music == MusicDriverTypes.Midi)
                        {
                            _reader.BaseStream.Seek(-8, SeekOrigin.Current);
                            return _reader.ReadBytes((int)size + 8);
                        }
                        _reader.BaseStream.Seek(size, SeekOrigin.Current);
                        break;
                    case "Mac0":
                        _reader.BaseStream.Seek(-12, SeekOrigin.Current);
                        size = _reader.ReadUInt32BigEndian() - 8;
                        return _reader.ReadBytes((int)size);
                    case "TOWS":
                        _reader.BaseStream.Seek(-12, SeekOrigin.Current);
                        size = _reader.ReadUInt32BigEndian();
                        return _reader.ReadBytes((int)size);
                    case "SOU ":
                        break;
                    default:
                        // skip
//                        Console.WriteLine("Skip Block: {0}", tag);
                        _reader.BaseStream.Seek(size, SeekOrigin.Current);
                        break;
                }
            } while (_reader.BaseStream.Position < pos);
            return null;
        }

        protected override ColorCycle[] ReadCYCL()
        {
            int j;
            var cycle = new List<ColorCycle>();
            while ((j = _reader.ReadByte()) != 0)
            {
                if (j < 1 || j > 16)
                {
                    throw new NotSupportedException(string.Format("Invalid color cycle index {0}", j));
                }
                var cycl = new ColorCycle();
                cycle.Add(cycl);

                _reader.ReadUInt16();

                cycl.Counter = 0;
                cycl.Delay = (ushort)(16384 / _reader.ReadUInt16BigEndian());
                cycl.Flags = _reader.ReadUInt16BigEndian();
                cycl.Start = _reader.ReadByte();
                cycl.End = _reader.ReadByte();
            }
            return cycle.ToArray();
        }

        public byte[] ReadCharset(long offset)
        {
            GotoResourceHeader(offset);
            var tag = ToTag(_reader.ReadBytes(4));
            if (tag != "CHAR")
                throw new NotSupportedException("Expected CHAR block.");

            var size = _reader.ReadUInt32BigEndian();
            _reader.BaseStream.Seek(-8, SeekOrigin.Current);
            var data = _reader.ReadBytes((int)(size));

            return data;
        }

        public override byte[] ReadScript(long offset)
        {
            GotoResourceHeader(offset);
            var block = ToTag(_reader.ReadBytes(4));
            var size = _reader.ReadUInt32BigEndian();

            if (block != "SCRP")
                throw new NotSupportedException("Expected SCRP block.");
            var data = _reader.ReadBytes((int)(size - 8));
            return data;
        }

        public override byte[] ReadCostume(long offset)
        {
            GotoResourceHeader(offset);
            var block = ToTag(_reader.ReadBytes(4));
            var size = _reader.ReadUInt32BigEndian();
            if (block != "COST")
                throw new NotSupportedException("Expected COST block.");
            return _reader.ReadBytes((int)size - 8);
        }

        protected override Dictionary<byte, long> ReadRoomOffsets()
        {
            var roomOffsets = new Dictionary<byte, long>();
            do
            {
                var block = ToTag(_reader.ReadBytes(4));
                var size = _reader.ReadUInt32BigEndian();

                switch (block)
                {
                // main container
                    case "LECF":
                        break;
                // room offset table
                    case "LOFF":
                        var numRooms = _reader.ReadByte();
                        while (numRooms-- != 0)
                        {
                            var room = _reader.ReadByte();
                            var offset = _reader.ReadUInt32();
                            roomOffsets[room] = offset;
                        }
                        return roomOffsets;
                    default:
                 // skip
//                        Console.WriteLine("Skip Block: {0}", block);
                        _reader.BaseStream.Seek(size - 8, SeekOrigin.Current);
                        break;
                }
            } while (_reader.BaseStream.Position < _reader.BaseStream.Length);
            return roomOffsets;
        }
    }
 
}
