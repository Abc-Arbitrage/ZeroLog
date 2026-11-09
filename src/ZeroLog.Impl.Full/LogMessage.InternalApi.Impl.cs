using System;
using System.Buffers;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Support;

namespace ZeroLog;

unsafe partial class LogMessage
{
    internal partial void InternalAppendChain(InternalApi.Chain chain)
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) <= _endOfBuffer && _objectIndex < _objects.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.Chain;
            _dataPointer += sizeof(ArgumentType);

            _objects[_objectIndex] = chain;

            *_dataPointer = _objectIndex;
            ++_dataPointer;

            ++_objectIndex;
        }
        else
        {
            TruncateMessage();
        }
    }

    internal partial void InternalAppendChainValue(string? value)
    {
        if (_dataPointer + sizeof(byte) <= _endOfBuffer)
        {
            if (value is not null)
            {
                if (_objectIndex < _objects.Length)
                {
                    _objects[_objectIndex] = value;

                    *_dataPointer = _objectIndex;
                    ++_dataPointer;

                    ++_objectIndex;
                }
                else
                {
                    TruncateMessage();
                }
            }
            else
            {
                *_dataPointer = InternalApi.Chain.NullStringIndex;
                ++_dataPointer;
            }
        }
        else
        {
            TruncateMessage();
        }
    }

    internal partial void InternalAppendChainValue<T>(T value)
        where T : unmanaged
    {
        if (_dataPointer + sizeof(T) <= _endOfBuffer)
        {
            *(T*)_dataPointer = value;
            _dataPointer += sizeof(T);
        }
        else
        {
            TruncateMessage();
        }
    }

    internal partial void InternalAppendChainValue<T>(T? value)
        where T : unmanaged
    {
        if (_dataPointer + 1 + sizeof(T) <= _endOfBuffer)
        {
            if (value is not null)
            {
                *_dataPointer = 1;
                ++_dataPointer;

                *(T*)_dataPointer = value.GetValueOrDefault();
                _dataPointer += sizeof(T);
            }
            else
            {
                *_dataPointer = 0;
                ++_dataPointer;
            }
        }
        else
        {
            TruncateMessage();
        }
    }

    internal partial void InternalAppendChainEnumValue<T>(T value)
        where T : struct, Enum
    {
        if (_dataPointer + sizeof(EnumArg) <= _endOfBuffer)
        {
            *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value));
            _dataPointer += sizeof(EnumArg);
        }
        else
        {
            TruncateMessage();
        }
    }

    internal partial void InternalAppendChainEnumValue<T>(T? value)
        where T : struct, Enum
    {
        if (_dataPointer + 1 + sizeof(EnumArg) <= _endOfBuffer)
        {
            if (value is not null)
            {
                *_dataPointer = 1;
                ++_dataPointer;

                *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value.GetValueOrDefault()));
                _dataPointer += sizeof(EnumArg);
            }
            else
            {
                *_dataPointer = 0;
                ++_dataPointer;
            }
        }
        else
        {
            TruncateMessage();
        }
    }

    partial class InternalApi
    {
        partial class Chain
        {
            public partial void Initialize()
            {
                // This makes sure the object is instantiated and the Execute method is JITted.

                lock (InitializationData.Lock) // Shouldn't be necessary, but future-proof
                {
                    using var data = InitializationData.Get();

                    var context = new ChainExecutionContext(
                        data.Message,
                        data.InputBuffer,
                        data.OutputBuffer,
                        FormatType.Unformatted,
                        ZeroLogConfiguration.Default,
                        data.KeyValueList
                    );

                    Execute(ref context);
                }
            }

            private sealed class InitializationData : IDisposable
            {
                public static readonly Lock Lock = new();
                private static readonly WeakReference<InitializationData> _weakInstance = new(null!);

                private byte[]? _inputBuffer;
                private char[]? _outputBuffer;

                public LogMessage Message { get; } = CreateTestMessage(LogLevel.Info, 1024, 32);
                public byte[] InputBuffer => _inputBuffer ??= ArrayPool<byte>.Shared.Rent(1024);
                public char[] OutputBuffer => _outputBuffer ??= ArrayPool<char>.Shared.Rent(1024);
                public KeyValueList KeyValueList { get; } = new(1024);

                private InitializationData() { }

                public static InitializationData Get()
                {
                    if (!_weakInstance.TryGetTarget(out var instance))
                    {
                        instance = new InitializationData();
                        _weakInstance.SetTarget(instance);
                    }

                    instance.Message.Initialize(null, LogLevel.Info);
                    instance.InputBuffer.AsSpan().Clear();
                    instance.KeyValueList.Clear();

                    return instance;
                }

                public void Dispose()
                {
                    if (_inputBuffer is { } inputBuffer)
                    {
                        _inputBuffer = null;
                        ArrayPool<byte>.Shared.Return(inputBuffer);
                    }

                    if (_outputBuffer is { } outputBuffer)
                    {
                        _outputBuffer = null;
                        ArrayPool<char>.Shared.Return(outputBuffer);
                    }
                }
            }
        }
    }
}
