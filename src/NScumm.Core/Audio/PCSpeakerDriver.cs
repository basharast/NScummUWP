﻿//
//  PcSpkDriver.cs
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
using NScumm.Core.Audio.SoftSynth;
using System.Diagnostics;
using System;

namespace NScumm.Core.Audio
{
    public class PCSpeakerDriver: EmulatedMidiDriver
    {
        public override bool IsStereo
        {
            get{ return _pcSpk.IsStereo; }
        }

        public override int Rate
        {
            get{ return _pcSpk.Rate; }
        }

        public PCSpeakerDriver(IMixer mixer)
            : base(mixer)
        {
            _pcSpk = new PCSpeaker(mixer.OutputRate);
            for (int i = 0; i < _channels.Length; i++)
            {
                _channels[i] = new MidiChannelPcSpk();
            }
        }

        public override int Property(int prop, int param)
        {
            return 0;
        }

        public override MidiDriverError Open()
        {
            if (IsOpen)
                return MidiDriverError.AlreadyOpen;

            base.Open();

            for (var i = 0; i < _channels.Length; ++i)
                _channels[i].Init(this, i);
            _activeChannel = null;
            _effectTimer = 0;
            _randBase = 1;

            // We need to take care we only send note frequencies, when the internal
            // settings actually changed, thus we need some extra state to keep track
            // of that.
            _lastActiveChannel = null;
            _lastActiveOut = 0;

            // We set the output sound type to music here to allow sound volume
            // adjustment. The drawback here is that we can not control the music and
            // sfx separately here. But the AdLib output has the same issue so it
            // should not be that bad.
            _mixerSoundHandle = _mixer.PlayStream(SoundType.Music, this, -1, Mixer.MaxChannelVolume, 0, false, true);
            return 0;
        }

        public void Close()
        {
            if (!IsOpen)
                return;
            _isOpen = false;

            _mixer.StopHandle(_mixerSoundHandle);
        }

        public override void Send(int b)
        {
            Debug.Assert((b & 0x0F) < 6);
            _channels[(b & 0x0F)].Send((uint)b);
        }

        protected override void GenerateSamples(short[] buf, int pos, int len)
        {
            // TODO: vs: do better than this, don't use an intermediate buffer
            var tmp = new short[len];
            _pcSpk.ReadBuffer(tmp, len);
            Array.Copy(tmp, 0, buf, pos, len);
        }

        public override MidiChannel AllocateChannel()
        {
            for (var i = 0; i < _channels.Length; i++)
            {
                if (_channels[i].Allocate())
                    return _channels[i];
            }
            return null;
        }

        public override MidiChannel GetPercussionChannel()
        {
            return null;
        }

        void UpdateNote()
        {
            int priority = 0;
            _activeChannel = null;
            for (var i = 0; i < 6; ++i)
            {
                if (_channels[i]._allocated && _channels[i]._out.active != 0 && _channels[i]._priority >= priority)
                {
                    priority = _channels[i]._priority;
                    _activeChannel = _channels[i];
                }
            }

            if (_activeChannel == null || _activeChannel._tl == 0)
            {
                _pcSpk.Stop();
                _lastActiveChannel = null;
                _lastActiveOut = 0;
            }
            else
            {
                Output((ushort)(_activeChannel._pitchBend + (_activeChannel._out.note << 7)));
            }
        }

        void Output(ushort output)
        {
            byte v1 = (byte)((output >> 7) & 0xFF);
            byte v2 = (byte)((output >> 2) & 0x1E);

            byte shift = _outputTable1[v1];
            var indexBase = _outputTable2[v1] << 5;
            var frequency = _frequencyTable[(indexBase + v2) / 2] >> shift;

            // Only output in case the active channel changed or the frequency changed.
            // This is not faithful to the original. Since our timings differ we would
            // get distorted sound otherwise though.
            if (_lastActiveChannel != _activeChannel || _lastActiveOut != output)
            {
                _pcSpk.Play(WaveForm.Square, 1193180 / frequency, -1);
                _lastActiveChannel = _activeChannel;
                _lastActiveOut = output;
            }
        }

        static byte GetEffectModifier(ushort level)
        {
            byte b = (byte)(level / 32);
            byte index = (byte)(level % 32);

            if (index == 0)
                return 0;

            return (byte)((b * (index + 1)) >> 5);
        }

        void SetupEffects(MidiChannelPcSpk chan, EffectEnvelope env, EffectDefinition def, byte flags, byte[] data, int offset)
        {
            def.phase = 0;
            def.useModWheel = (byte)(flags & 0x40);
            env.loop = (byte)(flags & 0x20);
            def.type = (short)(flags & 0x1F);

            env.modWheelSensitivity = 31;
            if (def.useModWheel != 0)
                env.modWheelState = (byte)(MidiChannelPcSpk.ModWheel >> 2);
            else
                env.modWheelState = 31;

            switch (def.type)
            {
                case 0:
                    env.maxLevel = 767;
                    env.startLevel = 383;
                    break;

                case 1:
                    env.maxLevel = 31;
                    env.startLevel = 15;
                    break;

                case 2:
                    env.maxLevel = 63;
                    env.startLevel = chan._out.unkB;
                    break;

                case 3:
                    env.maxLevel = 63;
                    env.startLevel = chan._out.unkC;
                    break;

                case 4:
                    env.maxLevel = 3;
                    env.startLevel = chan._instrument[4];
                    break;

                case 5:
                    env.maxLevel = 62;
                    env.startLevel = 31;
                    env.modWheelState = 0;
                    break;

                case 6:
                    env.maxLevel = 31;
                    env.startLevel = 0;
                    env.modWheelSensitivity = 0;
                    break;
            }

            StartEffect(env, data, offset);
        }

        void StartEffect(EffectEnvelope env, byte[] data, int offset)
        {
            env.state = 1;
            env.currentLevel = 0;
            env.modWheelLast = 31;
            env.duration = (short)(data[offset + 0] * 63);

            env.stateTargetLevels[0] = data[offset + 1];
            env.stateTargetLevels[1] = data[offset + 3];
            env.stateTargetLevels[2] = data[offset + 5];
            env.stateTargetLevels[3] = data[offset + 6];

            env.stateModWheelLevels[0] = data[offset + 2];
            env.stateModWheelLevels[1] = data[offset + 4];
            env.stateModWheelLevels[2] = 0;
            env.stateModWheelLevels[3] = data[offset + 7];

            InitNextEnvelopeState(env);
        }

        void InitNextEnvelopeState(EffectEnvelope env)
        {
            byte lastState = (byte)(env.state - 1);

            short stepCount = _effectEnvStepTable[GetEffectModifier((ushort)(((env.stateTargetLevels[lastState] & 0x7F) << 5) + env.modWheelSensitivity))];
            if ((env.stateTargetLevels[lastState] & 0x80) != 0)
                stepCount = GetRandScale(stepCount);
            if (stepCount == 0)
                stepCount = 1;

            env.stateNumSteps = env.stateStepCounter = stepCount;

            short totalChange = 0;
            if (lastState != 2)
            {
                totalChange = GetEffectModLevel(env.maxLevel, (sbyte)((env.stateModWheelLevels[lastState] & 0x7F) - 31));
                if ((env.stateModWheelLevels[lastState] & 0x80) != 0)
                    totalChange = GetRandScale(totalChange);

                if (totalChange + env.startLevel > env.maxLevel)
                    totalChange = (short)(env.maxLevel - env.startLevel);
                else if (totalChange + env.startLevel < 0)
                    totalChange = (short)-env.startLevel;

                totalChange -= env.currentLevel;
            }

            env.changePerStep = (short)(totalChange / stepCount);
            if (totalChange < 0)
            {
                totalChange = (short)-totalChange;
                env.dir = -1;
            }
            else
            {
                env.dir = 1;
            }
            env.changePerStepRem = (short)(totalChange % stepCount);
            env.changeCountRem = 0;
        }

        short GetRandScale(short input)
        {
            if ((_randBase & 1) != 0)
                _randBase = (_randBase >> 1) ^ 0xB8;
            else
                _randBase >>= 1;

            return (short)((_randBase * input) >> 8);
        }

        short GetEffectModLevel(short level, sbyte mod)
        {
            if (mod == 0)
            {
                return 0;
            }
            else if (mod == 31)
            {
                return level;
            }
            else if (level < -63 || level > 63)
            {
                return (short)((mod * (level + 1)) >> 6);
            }
            else if (mod < 0)
            {
                if (level < 0)
                    return GetEffectModifier((ushort)(((-level) << 5) - mod));
                else
                    return (short)-GetEffectModifier((ushort)((level << 5) - mod));
            }
            else
            {
                if (level < 0)
                    return (short)-GetEffectModifier((ushort)(((-level) << 5) + mod));
                else
                    return GetEffectModifier((ushort)(((-level) << 5) + mod));
            }
        }

        class MidiChannelPcSpk : MidiChannel
        {
            public void Init(PCSpeakerDriver owner, int channel)
            {
                _owner = owner;
                _channel = channel;
                _allocated = false;
            }

            public bool Allocate()
            {
                if (_allocated)
                    return false;

                _out = new OutputChannel();
                _instrument = new byte[23];
                _out.effectDefA.envelope = _out.effectEnvelopeA;
                _out.effectDefB.envelope = _out.effectEnvelopeB;

                _allocated = true;
                return true;
            }

            public override void Release()
            {
                _out.active = 0;
                _allocated = false;
                _owner.UpdateNote();
            }

            public override void Send(uint b)
            {
                byte type = (byte)(b & 0xF0);
                byte p1 = (byte)((b >> 8) & 0xFF);
                byte p2 = (byte)((b >> 16) & 0xFF);

                switch (type)
                {
                    case 0x80:
                        NoteOff(p1);
                        break;

                    case 0x90:
                        if (p2 != 0)
                            NoteOn(p1, p2);
                        else
                            NoteOff(p1);
                        break;

                    case 0xB0:
                        ControlChange(p1, p2);
                        break;

                    case 0xE0:
                        PitchBend((short)((p1 | (p2 << 7)) - 0x2000));
                        break;
                }
            }

            public override void NoteOff(byte note)
            {
                if (!_allocated)
                    return;

                if (_sustain != 0)
                {
                    if (_out.note == note)
                        _out.sustainNoteOff = 1;
                }
                else
                {
                    if (_out.note == note)
                    {
                        _out.active = 0;
                        _owner.UpdateNote();
                    }
                }
            }

            public override void NoteOn(byte note, byte velocity)
            {
                if (!_allocated)
                    return;

                _out.note = note;
                _out.sustainNoteOff = 0;
                _out.length = _instrument[0];

                if (_instrument[4] < PCSpeakerDriver._outInstrumentData.Length)
                    _out.instrument = PCSpeakerDriver._outInstrumentData[_instrument[4]];
                else
                    _out.instrument = null;

                _out.unkA = 0;
                _out.unkB = _instrument[1];
                _out.unkC = _instrument[2];
                _out.unkE = 0;
                _out.unk60 = 0;
                _out.active = 1;

                // In case we get a note on event on the last active channel, we reset the
                // last active channel, thus we assure the frequency is correctly set, even
                // when the same note was sent.
                if (_owner._lastActiveChannel == this)
                {
                    _owner._lastActiveChannel = null;
                    _owner._lastActiveOut = 0;
                }
                _owner.UpdateNote();

                _out.unkC += PCSpeakerDriver.GetEffectModifier((ushort)(_instrument[3] + ((velocity & 0xFE) << 4)));
                if (_out.unkC > 63)
                    _out.unkC = 63;

                if ((_instrument[5] & 0x80) != 0)
                    _owner.SetupEffects(this, _out.effectEnvelopeA, _out.effectDefA, _instrument[5], _instrument, 6);

                if ((_instrument[14] & 0x80) != 0)
                    _owner.SetupEffects(this, _out.effectEnvelopeB, _out.effectDefB, _instrument[14], _instrument, 15);
            }

            public override void ProgramChange(byte program)
            {
                // Nothing to implement here, the iMuse code takes care of passing us the
                // instrument data.
            }

            public override void PitchBend(short bend)
            {
                _pitchBend = (short)((bend * _pitchBendFactor) >> 6);
            }

            public override void ControlChange(byte control, byte value)
            {
                switch (control)
                {
                    case 1:
                        if (_out.effectEnvelopeA.state != 0 && _out.effectDefA.useModWheel != 0)
                            _out.effectEnvelopeA.modWheelState = (byte)(value >> 2);
                        if (_out.effectEnvelopeB.state != 0 && _out.effectDefB.useModWheel != 0)
                            _out.effectEnvelopeB.modWheelState = (byte)(value >> 2);
                        break;

                    case 7:
                        _tl = value;
                        if (_owner._activeChannel == this)
                        {
                            if (_tl == 0)
                            {
                                _owner._lastActiveChannel = null;
                                _owner._lastActiveOut = 0;
                                _owner._pcSpk.Stop();
                            }
                            else
                            {
                                _owner.Output((ushort)((_out.note << 7) + _pitchBend + _out.unk60 + _out.unkE));
                            }
                        }
                        break;

                    case 64:
                        _sustain = value;
                        if (value == 0 && _out.sustainNoteOff != 0)
                        {
                            _out.active = 0;
                            _owner.UpdateNote();
                        }
                        break;

                    case 123:
                        _out.active = 0;
                        _owner.UpdateNote();
                        break;
                }
            }

            public override void PitchBendFactor(byte value)
            {
                _pitchBendFactor = value;
            }

            public override void SysExCustomInstrument(uint type, byte[] instr)
            {
                Array.Copy(instr, _instrument, _instrument.Length);
            }

            public override void Priority(byte value)
            {
                _priority = value;
            }

            public override MidiDriver Device
            {
                get { return _owner; }
            }

            public override byte Number
            {
                get{ return (byte)_channel; }
            }

            internal byte[] _instrument;
            internal OutputChannel _out;
            internal bool _allocated;
            internal byte _priority;
            internal short _pitchBend;
            internal byte _tl;
            internal const byte ModWheel = 0;

            PCSpeakerDriver _owner;
            int _channel;
            byte _sustain;
            byte _pitchBendFactor;
        }

        class EffectEnvelope
        {
            public byte state;
            public short currentLevel;
            public short duration;
            public short maxLevel;
            public short startLevel;
            public byte loop;
            public byte[] stateTargetLevels = new byte[4];
            public byte[] stateModWheelLevels = new byte[4];
            public byte modWheelSensitivity;
            public byte modWheelState;
            public byte modWheelLast;
            public short stateNumSteps;
            public short stateStepCounter;
            public short changePerStep;
            public sbyte dir;
            public short changePerStepRem;
            public short changeCountRem;
        }

        class EffectDefinition
        {
            public short phase;
            public short type;
            public byte useModWheel;
            public EffectEnvelope envelope = new EffectEnvelope();
        }

        class OutputChannel
        {
            public byte active;
            public byte note;
            public byte sustainNoteOff;
            public byte length;
            public byte[] instrument;
            public byte unkA;
            public byte unkB;
            public byte unkC;
            public short unkE;
            public EffectEnvelope effectEnvelopeA = new EffectEnvelope();
            public EffectDefinition effectDefA = new EffectDefinition();
            public EffectEnvelope effectEnvelopeB = new EffectEnvelope();
            public EffectDefinition effectDefB = new EffectDefinition();
            public short unk60;
        }

        PCSpeaker _pcSpk;
        int _effectTimer;
        int _randBase;

        MidiChannelPcSpk[] _channels = new MidiChannelPcSpk[6];
        MidiChannelPcSpk _activeChannel;
        MidiChannelPcSpk _lastActiveChannel;
        ushort _lastActiveOut;

        static readonly byte[][] _outInstrumentData =
            {
                new byte[256]
                {
                    0x00, 0x03, 0x06, 0x09, 0x0C, 0x0F, 0x12, 0x15,
                    0x18, 0x1B, 0x1E, 0x21, 0x24, 0x27, 0x2A, 0x2D,
                    0x30, 0x33, 0x36, 0x39, 0x3B, 0x3E, 0x41, 0x43,
                    0x46, 0x49, 0x4B, 0x4E, 0x50, 0x52, 0x55, 0x57,
                    0x59, 0x5B, 0x5E, 0x60, 0x62, 0x64, 0x66, 0x67,
                    0x69, 0x6B, 0x6C, 0x6E, 0x70, 0x71, 0x72, 0x74,
                    0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7B,
                    0x7C, 0x7D, 0x7D, 0x7E, 0x7E, 0x7E, 0x7E, 0x7E,
                    0x7E, 0x7E, 0x7E, 0x7E, 0x7E, 0x7E, 0x7D, 0x7D,
                    0x7C, 0x7B, 0x7B, 0x7A, 0x79, 0x78, 0x77, 0x76,
                    0x75, 0x74, 0x72, 0x71, 0x70, 0x6E, 0x6C, 0x6B,
                    0x69, 0x67, 0x66, 0x64, 0x62, 0x60, 0x5E, 0x5B,
                    0x59, 0x57, 0x55, 0x52, 0x50, 0x4E, 0x4B, 0x49,
                    0x46, 0x43, 0x41, 0x3E, 0x3B, 0x39, 0x36, 0x33,
                    0x30, 0x2D, 0x2A, 0x27, 0x24, 0x21, 0x1E, 0x1B,
                    0x18, 0x15, 0x12, 0x0F, 0x0C, 0x09, 0x06, 0x03,

                    0x00, 0xFD, 0xFA, 0xF7, 0xF4, 0xF1, 0xEE, 0xEB,
                    0xE8, 0xE5, 0xE2, 0xDF, 0xDC, 0xD9, 0xD6, 0xD3,
                    0xD0, 0xCD, 0xCA, 0xC7, 0xC5, 0xC2, 0xBF, 0xBD,
                    0xBA, 0xB7, 0xB5, 0xB2, 0xB0, 0xAE, 0xAB, 0xA9,
                    0xA7, 0xA5, 0xA2, 0xA0, 0x9E, 0x9C, 0x9A, 0x99,
                    0x97, 0x95, 0x94, 0x92, 0x90, 0x8F, 0x8E, 0x8C,
                    0x8B, 0x8A, 0x89, 0x88, 0x87, 0x86, 0x85, 0x85,
                    0x84, 0x83, 0x83, 0x82, 0x82, 0x82, 0x82, 0x82,
                    0x82, 0x82, 0x82, 0x82, 0x82, 0x82, 0x83, 0x83,
                    0x84, 0x85, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A,
                    0x8B, 0x8C, 0x8E, 0x8F, 0x90, 0x92, 0x94, 0x95,
                    0x97, 0x99, 0x9A, 0x9C, 0x9E, 0xA0, 0xA2, 0xA5,
                    0xA7, 0xA9, 0xAB, 0xAE, 0xB0, 0xB2, 0xB5, 0xB7,
                    0xBA, 0xBD, 0xBF, 0xC2, 0xC5, 0xC7, 0xCA, 0xCD,
                    0xD0, 0xD3, 0xD6, 0xD9, 0xDC, 0xDF, 0xE2, 0xE5,
                    0xE8, 0xEB, 0xEE, 0xF1, 0xF4, 0xF7, 0xFA, 0xFD,
                },
                new byte[256]
                {
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                    0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                    0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                    0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                    0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
                    0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
                    0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
                    0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
                    0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47,
                    0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
                    0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57,
                    0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
                    0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67,
                    0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
                    0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77,
                    0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,

                    0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87,
                    0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
                    0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97,
                    0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
                    0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7,
                    0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
                    0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7,
                    0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,
                    0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7,
                    0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
                    0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7,
                    0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,
                    0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7,
                    0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF,
                    0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7,
                    0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF,
                },
                new byte[256]
                {
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                    0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88,
                
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                    0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78, 0x78,
                },
                new byte[256]
                {
                    0x29, 0x23, 0xBE, 0x84, 0xE1, 0x6C, 0xD6, 0xAE,
                    0x52, 0x90, 0x49, 0xF1, 0xF1, 0xBB, 0xE9, 0xEB,
                    0xB3, 0xA6, 0xDB, 0x3C, 0x87, 0x0C, 0x3E, 0x99,
                    0x24, 0x5E, 0x0D, 0x1C, 0x06, 0xB7, 0x47, 0xDE,
                    0xB3, 0x12, 0x4D, 0xC8, 0x43, 0xBB, 0x8B, 0xA6,
                    0x1F, 0x03, 0x5A, 0x7D, 0x09, 0x38, 0x25, 0x1F,
                    0x5D, 0xD4, 0xCB, 0xFC, 0x96, 0xF5, 0x45, 0x3B,
                    0x13, 0x0D, 0x89, 0x0A, 0x1C, 0xDB, 0xAE, 0x32,
                    0x20, 0x9A, 0x50, 0xEE, 0x40, 0x78, 0x36, 0xFD,
                    0x12, 0x49, 0x32, 0xF6, 0x9E, 0x7D, 0x49, 0xDC,
                    0xAD, 0x4F, 0x14, 0xF2, 0x44, 0x40, 0x66, 0xD0,
                    0x6B, 0xC4, 0x30, 0xB7, 0x32, 0x3B, 0xA1, 0x22,
                    0xF6, 0x22, 0x91, 0x9D, 0xE1, 0x8B, 0x1F, 0xDA,
                    0xB0, 0xCA, 0x99, 0x02, 0xB9, 0x72, 0x9D, 0x49,
                    0x2C, 0x80, 0x7E, 0xC5, 0x99, 0xD5, 0xE9, 0x80,
                    0xB2, 0xEA, 0xC9, 0xCC, 0x53, 0xBF, 0x67, 0xD6,

                    0xBF, 0x14, 0xD6, 0x7E, 0x2D, 0xDC, 0x8E, 0x66,
                    0x83, 0xEF, 0x57, 0x49, 0x61, 0xFF, 0x69, 0x8F,
                    0x61, 0xCD, 0xD1, 0x1E, 0x9D, 0x9C, 0x16, 0x72,
                    0x72, 0xE6, 0x1D, 0xF0, 0x84, 0x4F, 0x4A, 0x77,
                    0x02, 0xD7, 0xE8, 0x39, 0x2C, 0x53, 0xCB, 0xC9,
                    0x12, 0x1E, 0x33, 0x74, 0x9E, 0x0C, 0xF4, 0xD5,
                    0xD4, 0x9F, 0xD4, 0xA4, 0x59, 0x7E, 0x35, 0xCF,
                    0x32, 0x22, 0xF4, 0xCC, 0xCF, 0xD3, 0x90, 0x2D,
                    0x48, 0xD3, 0x8F, 0x75, 0xE6, 0xD9, 0x1D, 0x2A,
                    0xE5, 0xC0, 0xF7, 0x2B, 0x78, 0x81, 0x87, 0x44,
                    0x0E, 0x5F, 0x50, 0x00, 0xD4, 0x61, 0x8D, 0xBE,
                    0x7B, 0x05, 0x15, 0x07, 0x3B, 0x33, 0x82, 0x1F,
                    0x18, 0x70, 0x92, 0xDA, 0x64, 0x54, 0xCE, 0xB1,
                    0x85, 0x3E, 0x69, 0x15, 0xF8, 0x46, 0x6A, 0x04,
                    0x96, 0x73, 0x0E, 0xD9, 0x16, 0x2F, 0x67, 0x68,
                    0xD4, 0xF7, 0x4A, 0x4A, 0xD0, 0x57, 0x68, 0x76
                }
            };

        static readonly byte[] _outputTable1 =
            {
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2,
                2, 2, 2, 2, 3, 3, 3, 3,
                3, 3, 3, 3, 3, 3, 3, 3,
                4, 4, 4, 4, 4, 4, 4, 4,
                4, 4, 4, 4, 5, 5, 5, 5,
                5, 5, 5, 5, 5, 5, 5, 5,
                6, 6, 6, 6, 6, 6, 6, 6,
                6, 6, 6, 6, 7, 7, 7, 7,
                7, 7, 7, 7, 7, 7, 7, 7,
                7, 7, 7, 7, 7, 7, 7, 7
            };

        static readonly byte[] _outputTable2 =
            {
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7,
                8,  9, 10, 11,
                0,  1,  2,  3,
                4,  5,  6,  7
            };

        static readonly short[] _effectEnvStepTable =
            {
                1,    2,    4,    5,
                6,    7,    8,    9,
                10,   12,   14,   16,
                18,   21,   24,   30,
                36,   50,   64,   82,
                100,  136,  160,  192,
                240,  276,  340,  460,
                600,  860, 1200, 1600
            };

        static readonly ushort[] _frequencyTable =
            {
                0x8E84, 0x8E00, 0x8D7D, 0x8CFA,
                0x8C78, 0x8BF7, 0x8B76, 0x8AF5,
                0x8A75, 0x89F5, 0x8976, 0x88F7,
                0x8879, 0x87FB, 0x877D, 0x8700,
                0x8684, 0x8608, 0x858C, 0x8511,
                0x8496, 0x841C, 0x83A2, 0x8328,
                0x82AF, 0x8237, 0x81BF, 0x8147,
                0x80D0, 0x8059, 0x7FE3, 0x7F6D,
                0x7EF7, 0x7E82, 0x7E0D, 0x7D99,
                0x7D25, 0x7CB2, 0x7C3F, 0x7BCC,
                0x7B5A, 0x7AE8, 0x7A77, 0x7A06,
                0x7995, 0x7925, 0x78B5, 0x7846,
                0x77D7, 0x7768, 0x76FA, 0x768C,
                0x761F, 0x75B2, 0x7545, 0x74D9,
                0x746D, 0x7402, 0x7397, 0x732C,
                0x72C2, 0x7258, 0x71EF, 0x7186,
                0x711D, 0x70B5, 0x704D, 0x6FE5,
                0x6F7E, 0x6F17, 0x6EB0, 0x6E4A,
                0x6DE5, 0x6D7F, 0x6D1A, 0x6CB5,
                0x6C51, 0x6BED, 0x6B8A, 0x6B26,
                0x6AC4, 0x6A61, 0x69FF, 0x699D,
                0x693C, 0x68DB, 0x687A, 0x681A,
                0x67BA, 0x675A, 0x66FA, 0x669B,
                0x663D, 0x65DF, 0x6581, 0x6523,
                0x64C6, 0x6469, 0x640C, 0x63B0,
                0x6354, 0x62F8, 0x629D, 0x6242,
                0x61E7, 0x618D, 0x6133, 0x60D9,
                0x6080, 0x6027, 0x5FCE, 0x5F76,
                0x5F1E, 0x5EC6, 0x5E6E, 0x5E17,
                0x5DC1, 0x5D6A, 0x5D14, 0x5CBE,
                0x5C68, 0x5C13, 0x5BBE, 0x5B6A,
                0x5B15, 0x5AC1, 0x5A6E, 0x5A1A,
                0x59C7, 0x5974, 0x5922, 0x58CF,
                0x587D, 0x582C, 0x57DA, 0x5789,
                0x5739, 0x56E8, 0x5698, 0x5648,
                0x55F9, 0x55A9, 0x555A, 0x550B,
                0x54BD, 0x546F, 0x5421, 0x53D3,
                0x5386, 0x5339, 0x52EC, 0x52A0,
                0x5253, 0x5207, 0x51BC, 0x5170,
                0x5125, 0x50DA, 0x5090, 0x5046,
                0x4FFB, 0x4FB2, 0x4F68, 0x4F1F,
                0x4ED6, 0x4E8D, 0x4E45, 0x4DFC,
                0x4DB5, 0x4D6D, 0x4D25, 0x4CDE,
                0x4C97, 0x4C51, 0x4C0A, 0x4BC4,
                0x4B7E, 0x4B39, 0x4AF3, 0x4AAE,
                0x4A69, 0x4A24, 0x49E0, 0x499C,
                0x4958, 0x4914, 0x48D1, 0x488E,
                0x484B, 0x4808, 0x47C6, 0x4783
            };

    }
}

