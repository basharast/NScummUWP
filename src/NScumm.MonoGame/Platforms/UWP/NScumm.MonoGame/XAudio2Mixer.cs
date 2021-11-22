using NScumm.Core.Audio;
using NScumm.Core.Audio.SampleProviders;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using SharpDX;

namespace NScumm.MonoGame
{
    class XAudio2Mixer : IAudioOutput
    {
        class XAudioBuffer : IDisposable
        {
            private readonly byte[] _samples;
            private readonly DataStream _dataStream;

            public AudioBuffer Buffer { get; }

            public XAudioBuffer(int size)
            {
                _samples = new byte[size];
                _dataStream = DataStream.Create(_samples, true, true);
                Buffer = new AudioBuffer
                {
                    Stream = _dataStream,
                    AudioBytes = (int)_dataStream.Length,
                    Flags = BufferFlags.EndOfStream
                };
            }

            public void Dispose()
            {
                _dataStream.Dispose();
            }

            public bool FillWith(IAudioSampleProvider audioSampleProvider)
            {
                try
                {
                    if (_dataStream != null && audioSampleProvider != null)
                    {
                        Array.Clear(_samples, 0, _samples.Length);
                        audioSampleProvider?.Read(_samples, _samples.Length);
                        _dataStream.Position = 0;
                        _dataStream.Write(_samples, 0, _samples.Length);
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }
        }
        private const int NumBuffers = 8;
        private const int BufferSize = 4096 * 2 * 2;

        private IAudioSampleProvider _audioSampleProvider;
        private XAudio2 _xAudio;
        private SourceVoice _voice;

        private XAudioBuffer[] _buffers;

        private readonly MasteringVoice _masteringVoice;
        private int _index;
        private Task _audioThread;
        private bool _quit;

        public XAudio2Mixer()
        {
            _xAudio = new XAudio2();
            _xAudio.StartEngine();
            _masteringVoice = new MasteringVoice(_xAudio);
            _buffers = new XAudioBuffer[NumBuffers];

            for (var i = 0; i < NumBuffers; i++)
            {
                _buffers[i] = new XAudioBuffer(BufferSize);
            }
        }

        public void Play()
        {
            if (_voice != null)
            {
                _voice.Start();
            }
        }

        public void Pause()
        {
            Stop();
        }

        public void SetSampleProvider(IAudioSampleProvider audioSampleProvider)
        {
            var format = ToWaveFormat(audioSampleProvider.AudioFormat);
            _voice = new SourceVoice(_xAudio, format, true);
            _audioSampleProvider = audioSampleProvider;
            for (var i = 0; i < NumBuffers; i++)
            {
                FillBuffer(i);
            }
            _audioThread = Task.Factory.StartNew(OnAudioThread, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _voice.Start();
        }

        public void Dispose()
        {
            _quit = true;
            _audioThread.Wait();
            Stop();
            foreach (var buffer in _buffers)
            {
                buffer.Dispose();
            }
            _voice.Dispose();
            _masteringVoice.Dispose();
            _xAudio.Dispose();
        }

        public void Stop()
        {
            if (_voice != null)
            {
                _voice.Stop();
            }
        }

        private void OnAudioThread()
        {
            try
            {
                var state2 = true;
                while (!_quit && state2)
                {
                    var state = _voice.State;
                    while (state2 && state.BuffersQueued < NumBuffers)
                    {
                        state2 = FillBuffer(_index);
                        if (state2)
                        {
                            _index = ++_index % NumBuffers;
                            state = _voice.State;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private bool FillBuffer(int index)
        {
            var state = true;
            try
            {
                state = _buffers[index].FillWith(_audioSampleProvider);
                if (state)
                {
                    _voice.SubmitSourceBuffer(_buffers[index].Buffer, null);
                }
            }
            catch (Exception x)
            {

            }
            return state;
        }

        private static WaveFormat ToWaveFormat(AudioFormat format)
        {
            return new WaveFormat(format.SampleRate, format.Channels);
        }
    }
}

