using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog;

unsafe partial class LogMessage
{
    private bool TryWriteChain(InternalApi.Chain? chain,
                               ref byte* dataPointer,
                               Span<char> outputBuffer,
                               ZeroLogConfiguration config,
                               FormatType formatType,
                               KeyValueList? keyValueList,
                               out int charsWritten)
    {
        if (chain is null)
        {
            charsWritten = 0;
            return false;
        }

        var inputData = new ReadOnlySpan<byte>(dataPointer, (int)(_dataPointer - dataPointer));

        var context = new InternalApi.Chain.ChainExecutionContext(
            this,
            inputData,
            outputBuffer,
            formatType,
            config,
            keyValueList
        );

        chain.CallExecute(ref context);

        var success = context.Complete(out var dataRead, out charsWritten);
        dataPointer += dataRead;
        return success;
    }

#pragma warning disable CS1591

    partial class InternalApi
    {
        partial class Chain
        {
            // Index 0 is safe to use to designate a null string, since the chain object itself needs to occupy at least one slot.
            internal const byte NullStringIndex = 0;

            ref partial struct ChainExecutionContext
            {
                private readonly LogMessage _logMessage;
                private readonly ZeroLogConfiguration _config;
                private readonly FormatType _formatType;
                private readonly KeyValueList? _keyValueList;
                private readonly int _initialInputDataLength;
                private readonly int _initialOutputBufferLength;

                private ReadOnlySpan<byte> _inputData;
                private Span<char> _outputBuffer;
                private bool _isTruncated = false;
                private ReadOnlySpan<char> _format;
                private string? _key;

                internal ChainExecutionContext(LogMessage logMessage,
                                               ReadOnlySpan<byte> inputData,
                                               Span<char> outputBuffer,
                                               FormatType formatType,
                                               ZeroLogConfiguration config,
                                               KeyValueList? keyValueList)
                {
                    _logMessage = logMessage;
                    _config = config;
                    _formatType = formatType;
                    _keyValueList = keyValueList;

                    _inputData = inputData;
                    _outputBuffer = outputBuffer;

                    _initialInputDataLength = inputData.Length;
                    _initialOutputBufferLength = outputBuffer.Length;

                    ResetStatus();
                }

                internal bool Complete(out int dataRead, out int charsWritten)
                {
                    dataRead = _initialInputDataLength - _inputData.Length;
                    charsWritten = _initialOutputBufferLength - _outputBuffer.Length;
                    return !_isTruncated;
                }

                private void ResetStatus()
                {
                    _format = ReadOnlySpan<char>.Empty;
                    _key = null;
                }

                public partial void OutputString()
                {
                    if (TryReadVariableString(out var value))
                        OutputString(value);
                }

                public partial void OutputString(string? value)
                {
                    if (_isTruncated)
                        return;

                    if (_key is null)
                    {
                        value ??= _config.NullDisplayString ?? string.Empty;
                        var outputBuffer = _outputBuffer;

                        if (value.Length > 0)
                        {
                            var charsToCopy = Math.Min(value.Length, outputBuffer.Length);
                            value.AsSpan().CopyTo(outputBuffer.Slice(0, charsToCopy));

                            if (_key is null)
                                _outputBuffer = outputBuffer.Slice(charsToCopy);

                            if (charsToCopy < value.Length)
                                _isTruncated = true;
                        }
                    }
                    else if (_keyValueList is not null)
                    {
                        if (!_keyValueList.Add(_key, value))
                            _isTruncated = true;
                    }

                    ResetStatus();
                }

                public partial void OutputNullableValueType<T>()
                    where T : unmanaged
                {
                    if (_isTruncated || _inputData.Length < sizeof(byte))
                        goto truncate;

                    var hasValue = _inputData[0];
                    _inputData = _inputData.Slice(sizeof(byte));

                    switch (hasValue)
                    {
                        case 0:
                            OutputString(null);
                            return;

                        case 1:
                            OutputValueType<T>();
                            return;
                    }

                    truncate:
                    _isTruncated = true;
                }

                public partial void OutputValueType<T>()
                    where T : unmanaged
                {
                    if (_isTruncated || _inputData.Length < sizeof(T))
                        goto truncate;

                    var outputBuffer = _key is null
                        ? _outputBuffer
                        : _keyValueList is not null
                            ? _keyValueList.GetRemainingBuffer()
                            : Span<char>.Empty;

                    if (_key is not null)
                    {
                        _format = typeof(T) == typeof(DateTime) ? KeyValueFormatStrings.DateTime
                            : typeof(T) == typeof(TimeSpan) ? KeyValueFormatStrings.TimeSpan
                            : typeof(T) == typeof(DateOnly) ? KeyValueFormatStrings.DateOnly
                            : typeof(T) == typeof(TimeOnly) ? KeyValueFormatStrings.TimeOnly
                            : typeof(T) == typeof(DateTimeOffset) ? KeyValueFormatStrings.DateTimeOffset
                            : ReadOnlySpan<char>.Empty;
                    }

                    if (typeof(T) == typeof(bool) ? MemoryMarshal.Read<bool>(_inputData).TryFormat(outputBuffer, out var charsWritten)
                        : typeof(T) == typeof(byte) ? MemoryMarshal.Read<byte>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(sbyte) ? MemoryMarshal.Read<sbyte>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(short) ? MemoryMarshal.Read<short>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(ushort) ? MemoryMarshal.Read<ushort>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(int) ? MemoryMarshal.Read<int>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(uint) ? MemoryMarshal.Read<uint>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(long) ? MemoryMarshal.Read<long>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(ulong) ? MemoryMarshal.Read<ulong>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(nint) ? MemoryMarshal.Read<nint>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(nuint) ? MemoryMarshal.Read<nuint>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(float) ? MemoryMarshal.Read<float>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(double) ? MemoryMarshal.Read<double>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(decimal) ? MemoryMarshal.Read<decimal>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(Guid) ? MemoryMarshal.Read<Guid>(_inputData).TryFormat(outputBuffer, out charsWritten, _format)
                        : typeof(T) == typeof(DateTime) ? MemoryMarshal.Read<DateTime>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(TimeSpan) ? MemoryMarshal.Read<TimeSpan>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(DateOnly) ? MemoryMarshal.Read<DateOnly>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(TimeOnly) ? MemoryMarshal.Read<TimeOnly>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(DateTimeOffset) ? MemoryMarshal.Read<DateTimeOffset>(_inputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(char) && TryFormatChar(outputBuffer, MemoryMarshal.Read<char>(_inputData), out charsWritten))
                    {
                        var rawData = _inputData.Slice(0, sizeof(T));
                        _inputData = _inputData.Slice(sizeof(T));

                        if (_key is null)
                        {
                            _outputBuffer = _outputBuffer.Slice(charsWritten);
                        }
                        else
                        {
                            if (_keyValueList?.Add(_key, ArgumentType.FromValueType<T>(), charsWritten, rawData) is false)
                                _isTruncated = true;
                        }

                        ResetStatus();
                        return;
                    }

                    if (_keyValueList is null && _key is not null)
                    {
                        _inputData = _inputData.Slice(sizeof(T));
                        ResetStatus();
                        return;
                    }

                    truncate:
                    _isTruncated = true;
                    return;

                    static bool TryFormatChar(Span<char> outputBuffer, char value, out int charsWritten)
                    {
                        if (outputBuffer.Length > 0)
                        {
                            outputBuffer[0] = value;
                            charsWritten = 1;
                            return true;
                        }

                        charsWritten = 0;
                        return false;
                    }
                }

                public partial void OutputNullableEnum()
                {
                    if (_isTruncated || _inputData.Length < sizeof(byte))
                        goto truncate;

                    var hasValue = _inputData[0];
                    _inputData = _inputData.Slice(sizeof(byte));

                    switch (hasValue)
                    {
                        case 0:
                            OutputString(null);
                            return;

                        case 1:
                            OutputEnum();
                            return;
                    }

                    truncate:
                    _isTruncated = true;
                }

                public partial void OutputEnum()
                {
                    if (_isTruncated || _inputData.Length < sizeof(EnumArg))
                        goto truncate;

                    var outputBuffer = _key is null
                        ? _outputBuffer
                        : _keyValueList is not null
                            ? _keyValueList.GetRemainingBuffer()
                            : Span<char>.Empty;

                    ref readonly var enumArg = ref Unsafe.As<byte, EnumArg>(ref MemoryMarshal.GetReference(_inputData));

                    if (enumArg.TryFormat(outputBuffer, out var charsWritten, _config))
                    {
                        var rawData = _inputData.Slice(0, sizeof(EnumArg));
                        _inputData = _inputData.Slice(sizeof(EnumArg));

                        if (_key is null)
                        {
                            _outputBuffer = _outputBuffer.Slice(charsWritten);
                        }
                        else
                        {
                            if (_keyValueList?.Add(_key, ArgumentType.Enum, charsWritten, rawData) is false)
                                _isTruncated = true;
                        }

                        ResetStatus();
                        return;
                    }

                    if (_keyValueList is null && _key is not null)
                    {
                        _inputData = _inputData.Slice(sizeof(EnumArg));
                        ResetStatus();
                        return;
                    }

                    truncate:
                    _isTruncated = true;
                }

                public partial void SetFormatString()
                {
                    if (TryReadVariableString(out var format))
                        SetFormatString(format);
                }

                public partial void SetFormatString(string? format)
                {
                    _format = _formatType is FormatType.Formatted
                        ? format
                        : ReadOnlySpan<char>.Empty;
                }

                public partial void SetKeyString()
                {
                    if (TryReadVariableString(out var key))
                        SetKeyString(key);
                }

                public partial void SetKeyString(string? key)
                {
                    // This is only called from an AppendKeyValue call, so we always set _key to a non-null instance
                    _key = key ?? string.Empty;
                }

                private bool TryReadVariableString(out string? value)
                {
                    value = null;

                    if (_isTruncated || _inputData.Length < sizeof(byte))
                        goto truncate;

                    var stringIndex = _inputData[0];
                    _inputData = _inputData.Slice(sizeof(byte));

                    if (stringIndex == NullStringIndex)
                        return true;

                    if (stringIndex >= _logMessage._objects.Length)
                        goto truncate;

                    value = _logMessage._objects[stringIndex] as string;

                    if (value is not null)
                        return true;

                    truncate:
                    _isTruncated = true;
                    return false;
                }
            }
        }
    }

    internal static void Warmup()
        => WarmupChain.PerformWarmup();

    private class WarmupChain : InternalApi.Chain
    {
        public static void PerformWarmup()
        {
            var logMessage = new LogMessage("");
            var config = new ZeroLogConfiguration();

            Span<byte> input = stackalloc byte[512];
            Span<char> output = stackalloc char[512];

            input.Clear();

            var context = new ChainExecutionContext(
                logMessage,
                input,
                output,
                default,
                config,
                null
            );

            var chain = new WarmupChain();
            chain.CallExecute(ref context);
        }

        protected override void Execute(ref ChainExecutionContext context)
        {
            // Pre-JIT these

            context.OutputValueType<char>();
            context.OutputValueType<bool>();
            context.OutputValueType<byte>();
            context.OutputValueType<sbyte>();
            context.OutputValueType<short>();
            context.OutputValueType<ushort>();
            context.OutputValueType<int>();
            context.OutputValueType<uint>();
            context.OutputValueType<long>();
            context.OutputValueType<ulong>();
            context.OutputValueType<nint>();
            context.OutputValueType<nuint>();
            context.OutputValueType<float>();
            context.OutputValueType<double>();
            context.OutputValueType<decimal>();
            context.OutputValueType<Guid>();
            context.OutputValueType<DateTime>();
            context.OutputValueType<TimeSpan>();
            context.OutputValueType<DateOnly>();
            context.OutputValueType<TimeOnly>();
            context.OutputValueType<DateTimeOffset>();

            context.OutputNullableValueType<char>();
            context.OutputNullableValueType<bool>();
            context.OutputNullableValueType<byte>();
            context.OutputNullableValueType<sbyte>();
            context.OutputNullableValueType<short>();
            context.OutputNullableValueType<ushort>();
            context.OutputNullableValueType<int>();
            context.OutputNullableValueType<uint>();
            context.OutputNullableValueType<long>();
            context.OutputNullableValueType<ulong>();
            context.OutputNullableValueType<nint>();
            context.OutputNullableValueType<nuint>();
            context.OutputNullableValueType<float>();
            context.OutputNullableValueType<double>();
            context.OutputNullableValueType<decimal>();
            context.OutputNullableValueType<Guid>();
            context.OutputNullableValueType<DateTime>();
            context.OutputNullableValueType<TimeSpan>();
            context.OutputNullableValueType<DateOnly>();
            context.OutputNullableValueType<TimeOnly>();
            context.OutputNullableValueType<DateTimeOffset>();

            context.OutputString();
            context.OutputString("");
        }
    }
}
