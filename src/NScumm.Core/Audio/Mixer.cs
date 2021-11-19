﻿//
//  Mixer.cs
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

using System;
using System.Diagnostics;
using NScumm.Core.Audio.SampleProviders;

namespace NScumm.Core.Audio
{
    public class Mixer : AudioSampleProvider16, IMixer
    {
        private const int NumChannels = 16;
        public const int MaxMixerVolume = 256;
        public const int MaxChannelVolume = 255;

        private readonly Channel[] _channels;
        private readonly object _gate = new object();
        private int _handleSeed;
        private readonly SoundTypeSettings[] soundTypeSettings;

        public Mixer(int sampleRate)
        {
            Debug.Assert(sampleRate > 0);
            _channels = new Channel[NumChannels];
            soundTypeSettings = new SoundTypeSettings[4];
            for (var i = 0; i < soundTypeSettings.Length; i++)
            {
                soundTypeSettings[i] = new SoundTypeSettings(MaxMixerVolume);
            }
            OutputRate = sampleRate;
            AudioFormat = new AudioFormat(OutputRate);
        }

        public override AudioFormat AudioFormat { get; }

        public int OutputRate { get; }

        public bool IsReady { get; private set; }

        public SoundHandle PlayStream(SoundType type, IAudioStream stream, int id = -1, int volume = 255,
            int balance = 0, bool autofreeStream = true, bool permanent = false, bool reverseStereo = false)
        {
            lock (_gate)
            {
                if (stream == null)
                {
                    //                    Console.Error.WriteLine("stream is null");
                    return new SoundHandle();
                }

                Debug.Assert(IsReady);

                // Prevent duplicate sounds
                if (id != -1)
                {
                    for (var i = 0; i != NumChannels; i++)
                        if (_channels[i] != null && _channels[i].Id == id)
                        {
                            // Delete the stream if were asked to auto-dispose it.
                            // Note: This could cause trouble if the client code does not
                            // yet expect the stream to be gone. The primary example to
                            // keep in mind here is QueuingAudioStream.
                            // Thus, as a quick rule of thumb, you should never, ever,
                            // try to play QueuingAudioStreams with a sound id.
                            if (autofreeStream)
                                stream.Dispose();
                            return new SoundHandle();
                        }
                }

                // Create the channel
                var chan = new Channel(this, type, stream, autofreeStream, reverseStereo, id, permanent)
                {
                    Volume = volume,
                    Balance = balance
                };
                return InsertChannel(chan);
            }
        }

        public void StopID(int id)
        {
            lock (_gate)
            {
                for (var i = 0; i != NumChannels; i++)
                {
                    if (_channels[i] != null && _channels[i].Id == id)
                    {
                        _channels[i] = null;
                    }
                }
            }
        }

        public void StopHandle(SoundHandle handle)
        {
            lock (_gate)
            {
                // Simply ignore stop requests for handles of sounds that already terminated
                var index = handle.Value % NumChannels;
                if (_channels[index] == null || _channels[index].Handle.Value != handle.Value)
                    return;

                _channels[index] = null;
            }
        }

        public bool IsSoundHandleActive(SoundHandle handle)
        {
            lock (_gate)
            {
                var index = handle.Value % NumChannels;
                return _channels[index] != null && _channels[index].Handle.Value == handle.Value;
            }
        }

        public bool IsSoundIdActive(int id)
        {
            lock (_gate)
            {
                for (var i = 0; i != NumChannels; i++)
                    if (_channels[i] != null && _channels[i].Id == id)
                        return true;
                return false;
            }
        }

        public bool HasActiveChannelOfType(SoundType type)
        {
            lock (_gate)
            {
                for (var i = 0; i != NumChannels; i++)
                    if (_channels[i] != null && _channels[i].Type == type)
                        return true;
                return false;
            }
        }

        public int GetVolumeForSoundType(SoundType type)
        {
            Debug.Assert(0 <= type && (int)type < soundTypeSettings.Length);
            return soundTypeSettings[(int)type].Volume;
        }

        public void PauseHandle(SoundHandle handle, bool paused)
        {
            lock (_gate)
            {
                // Simply ignore (un)pause requests for sounds that already terminated
                var index = handle.Value % NumChannels;
                if (_channels[index] == null || _channels[index].Handle.Value != handle.Value)
                    return;

                _channels[index].Pause(paused);
            }
        }

        public void PauseAll(bool pause)
        {
            lock (_gate)
            {
                for (var i = 0; i != _channels.Length; i++)
                {
                    if (_channels[i] != null)
                    {
                        _channels[i].Pause(pause);
                    }
                }
            }
        }

        public void StopAll()
        {
            lock (_gate)
            {
                for (var i = 0; i != _channels.Length; i++)
                {
                    if (_channels[i] != null && !_channels[i].IsPermanent)
                    {
                        _channels[i] = null;
                    }
                }
            }
        }

        public int GetSoundElapsedTime(SoundHandle handle)
        {
            return GetElapsedTime(handle).Milliseconds;
        }

        public void SetChannelVolume(SoundHandle handle, int volume)
        {
            lock (_gate)
            {
                var index = handle.Value % NumChannels;
                if (_channels[index] == null || _channels[index].Handle.Value != handle.Value)
                    return;

                _channels[index].Volume = volume;
            }
        }

        public int GetChannelVolume(SoundHandle handle)
        {
            var index = handle.Value % NumChannels;
            if (_channels[index] == null || _channels[index].Handle.Value != handle.Value)
                return 0;
            return _channels[index].Volume;
        }

        public void SetChannelBalance(SoundHandle handle, int balance)
        {
            lock (_gate)
            {
                var index = handle.Value % NumChannels;
                if (_channels[index] == null || _channels[index].Handle.Value != handle.Value)
                    return;

                _channels[index].Balance = balance;
            }
        }

        public int GetChannelBalance(SoundHandle handle)
        {
            var index = handle.Value % NumChannels;
            if (_channels[index] == null || _channels[index].Handle.Value != handle.Value)
                return 0;

            return _channels[index].Balance;
        }

        public override int Read(short[] samples, int count)
        {
            Debug.Assert(samples != null);

            lock (_gate)
            {
                // we store stereo, 16-bit samples
                Debug.Assert(count % 2 == 0);

                // Since the mixer callback has been called, the mixer must be ready...
                IsReady = true;

                // mix all channels
                int res = 0, tmp;
                for (var i = 0; i != NumChannels; i++)
                    if (_channels[i] != null)
                    {
                        if (_channels[i].IsFinished)
                        {
                            _channels[i] = null;
                        }
                        else if (!_channels[i].IsPaused)
                        {
                            tmp = _channels[i].Mix(samples, count);

                            if (tmp > res)
                                res = tmp;
                        }
                    }

                return res * 2;
            }
        }

        public void PauseId(int id, bool paused)
        {
            lock (_gate)
            {
                for (var i = 0; i != NumChannels; i++)
                {
                    if (_channels[i] != null && _channels[i].Id == id)
                    {
                        _channels[i].Pause(paused);
                        return;
                    }
                }
            }
        }

        public bool IsSoundTypeMuted(SoundType type)
        {
            Debug.Assert(0 <= type && (int)type < soundTypeSettings.Length);
            return soundTypeSettings[(int)type].Mute;
        }

        private Timestamp GetElapsedTime(SoundHandle handle)
        {
            lock (_gate)
            {
                var index = handle.Value % NumChannels;
                if (_channels[index] == null || _channels[index].Handle.Value != handle.Value)
                    return new Timestamp(0, OutputRate);

                return _channels[index].GetElapsedTime();
            }
        }

        private SoundHandle InsertChannel(Channel chan)
        {
            var index = -1;
            for (var i = 0; i != NumChannels; i++)
            {
                if (_channels[i] == null)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                throw new InvalidOperationException("MixerImpl::out of mixer slots");
            }

            _channels[index] = chan;

            var chanHandle = new SoundHandle
            {
                Value = index + _handleSeed * NumChannels
            };
            chan.Handle = chanHandle;
            _handleSeed++;
            return chanHandle;
        }

        private struct SoundTypeSettings
        {
            public SoundTypeSettings(int volume)
                : this()
            {
                Volume = volume;
            }

            public bool Mute;
            public readonly int Volume;
        }
    }
}