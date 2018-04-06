using System;
using System.Runtime.CompilerServices;

namespace ZeroLog
{
    partial interface ILogEvent
    {
        ILogEvent Append(bool value);
        ILogEvent Append(bool? value);
        ILogEvent Append(byte value);
        ILogEvent Append(byte? value);
        ILogEvent Append(byte value, string format);
        ILogEvent Append(byte? value, string format);
        ILogEvent Append(char value);
        ILogEvent Append(char? value);
        ILogEvent Append(short value);
        ILogEvent Append(short? value);
        ILogEvent Append(short value, string format);
        ILogEvent Append(short? value, string format);
        ILogEvent Append(int value);
        ILogEvent Append(int? value);
        ILogEvent Append(int value, string format);
        ILogEvent Append(int? value, string format);
        ILogEvent Append(long value);
        ILogEvent Append(long? value);
        ILogEvent Append(long value, string format);
        ILogEvent Append(long? value, string format);
        ILogEvent Append(float value);
        ILogEvent Append(float? value);
        ILogEvent Append(float value, string format);
        ILogEvent Append(float? value, string format);
        ILogEvent Append(double value);
        ILogEvent Append(double? value);
        ILogEvent Append(double value, string format);
        ILogEvent Append(double? value, string format);
        ILogEvent Append(decimal value);
        ILogEvent Append(decimal? value);
        ILogEvent Append(decimal value, string format);
        ILogEvent Append(decimal? value, string format);
        ILogEvent Append(Guid value);
        ILogEvent Append(Guid? value);
        ILogEvent Append(Guid value, string format);
        ILogEvent Append(Guid? value, string format);
        ILogEvent Append(DateTime value);
        ILogEvent Append(DateTime? value);
        ILogEvent Append(DateTime value, string format);
        ILogEvent Append(DateTime? value, string format);
        ILogEvent Append(TimeSpan value);
        ILogEvent Append(TimeSpan? value);
        ILogEvent Append(TimeSpan value, string format);
        ILogEvent Append(TimeSpan? value, string format);
    }

    unsafe partial class LogEvent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(bool value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(bool)))
                return this;

            AppendArgumentType(ArgumentType.Boolean);
            AppendBoolean(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(bool? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(bool)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Boolean);
            AppendBoolean(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(byte value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte)))
                return this;

            AppendArgumentType(ArgumentType.Byte);
            AppendByte(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(byte? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Byte);
            AppendByte(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(byte value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(byte)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.Byte);
            AppendString(format);
            AppendByte(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(byte? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(byte)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.Byte);
            AppendString(format);
            AppendByte(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(char value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(char)))
                return this;

            AppendArgumentType(ArgumentType.Char);
            AppendChar(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(char? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(char)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Char);
            AppendChar(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(short value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(short)))
                return this;

            AppendArgumentType(ArgumentType.Int16);
            AppendInt16(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(short? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(short)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Int16);
            AppendInt16(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(short value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(short)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.Int16);
            AppendString(format);
            AppendInt16(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(short? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(short)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.Int16);
            AppendString(format);
            AppendInt16(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(int value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(int)))
                return this;

            AppendArgumentType(ArgumentType.Int32);
            AppendInt32(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(int? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(int)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Int32);
            AppendInt32(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(int value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(int)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.Int32);
            AppendString(format);
            AppendInt32(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(int? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(int)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.Int32);
            AppendString(format);
            AppendInt32(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(long value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(long)))
                return this;

            AppendArgumentType(ArgumentType.Int64);
            AppendInt64(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(long? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(long)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Int64);
            AppendInt64(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(long value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(long)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.Int64);
            AppendString(format);
            AppendInt64(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(long? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(long)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.Int64);
            AppendString(format);
            AppendInt64(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(float value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(float)))
                return this;

            AppendArgumentType(ArgumentType.Single);
            AppendSingle(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(float? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(float)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Single);
            AppendSingle(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(float value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(float)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.Single);
            AppendString(format);
            AppendSingle(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(float? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(float)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.Single);
            AppendString(format);
            AppendSingle(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(double value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(double)))
                return this;

            AppendArgumentType(ArgumentType.Double);
            AppendDouble(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(double? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(double)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Double);
            AppendDouble(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(double value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(double)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.Double);
            AppendString(format);
            AppendDouble(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(double? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(double)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.Double);
            AppendString(format);
            AppendDouble(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(decimal value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(decimal)))
                return this;

            AppendArgumentType(ArgumentType.Decimal);
            AppendDecimal(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(decimal? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(decimal)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Decimal);
            AppendDecimal(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(decimal value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(decimal)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.Decimal);
            AppendString(format);
            AppendDecimal(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(decimal? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(decimal)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.Decimal);
            AppendString(format);
            AppendDecimal(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(Guid value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(Guid)))
                return this;

            AppendArgumentType(ArgumentType.Guid);
            AppendGuid(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(Guid? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(Guid)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Guid);
            AppendGuid(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(Guid value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(Guid)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.Guid);
            AppendString(format);
            AppendGuid(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(Guid? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(Guid)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.Guid);
            AppendString(format);
            AppendGuid(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(DateTime value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(DateTime)))
                return this;

            AppendArgumentType(ArgumentType.DateTime);
            AppendDateTime(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(DateTime? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(DateTime)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.DateTime);
            AppendDateTime(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(DateTime value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(DateTime)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.DateTime);
            AppendString(format);
            AppendDateTime(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(DateTime? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(DateTime)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.DateTime);
            AppendString(format);
            AppendDateTime(value.GetValueOrDefault());
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(TimeSpan value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(TimeSpan)))
                return this;

            AppendArgumentType(ArgumentType.TimeSpan);
            AppendTimeSpan(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(TimeSpan? value)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(TimeSpan)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.TimeSpan);
            AppendTimeSpan(value.GetValueOrDefault());
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(TimeSpan value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(TimeSpan)))
                return this;

            AppendArgumentTypeWithFormat(ArgumentType.TimeSpan);
            AppendString(format);
            AppendTimeSpan(value);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(TimeSpan? value, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(TimeSpan)))
                return this;

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentTypeWithFormat(ArgumentType.TimeSpan);
            AppendString(format);
            AppendTimeSpan(value.GetValueOrDefault());
            return this;
        }

    }

    partial class NoopLogEvent
    {
        public ILogEvent Append(bool value) => this;
        public ILogEvent Append(bool? value) => this;
        public ILogEvent Append(byte value) => this;
        public ILogEvent Append(byte? value) => this;
        public ILogEvent Append(byte value, string format) => this;
        public ILogEvent Append(byte? value, string format) => this;
        public ILogEvent Append(char value) => this;
        public ILogEvent Append(char? value) => this;
        public ILogEvent Append(short value) => this;
        public ILogEvent Append(short? value) => this;
        public ILogEvent Append(short value, string format) => this;
        public ILogEvent Append(short? value, string format) => this;
        public ILogEvent Append(int value) => this;
        public ILogEvent Append(int? value) => this;
        public ILogEvent Append(int value, string format) => this;
        public ILogEvent Append(int? value, string format) => this;
        public ILogEvent Append(long value) => this;
        public ILogEvent Append(long? value) => this;
        public ILogEvent Append(long value, string format) => this;
        public ILogEvent Append(long? value, string format) => this;
        public ILogEvent Append(float value) => this;
        public ILogEvent Append(float? value) => this;
        public ILogEvent Append(float value, string format) => this;
        public ILogEvent Append(float? value, string format) => this;
        public ILogEvent Append(double value) => this;
        public ILogEvent Append(double? value) => this;
        public ILogEvent Append(double value, string format) => this;
        public ILogEvent Append(double? value, string format) => this;
        public ILogEvent Append(decimal value) => this;
        public ILogEvent Append(decimal? value) => this;
        public ILogEvent Append(decimal value, string format) => this;
        public ILogEvent Append(decimal? value, string format) => this;
        public ILogEvent Append(Guid value) => this;
        public ILogEvent Append(Guid? value) => this;
        public ILogEvent Append(Guid value, string format) => this;
        public ILogEvent Append(Guid? value, string format) => this;
        public ILogEvent Append(DateTime value) => this;
        public ILogEvent Append(DateTime? value) => this;
        public ILogEvent Append(DateTime value, string format) => this;
        public ILogEvent Append(DateTime? value, string format) => this;
        public ILogEvent Append(TimeSpan value) => this;
        public ILogEvent Append(TimeSpan? value) => this;
        public ILogEvent Append(TimeSpan value, string format) => this;
        public ILogEvent Append(TimeSpan? value, string format) => this;
    }

    partial class ForwardingLogEvent
    {
        public ILogEvent Append(bool value) => this;
        public ILogEvent Append(bool? value) => this;
        public ILogEvent Append(byte value) => this;
        public ILogEvent Append(byte? value) => this;
        public ILogEvent Append(byte value, string format) => this;
        public ILogEvent Append(byte? value, string format) => this;
        public ILogEvent Append(char value) => this;
        public ILogEvent Append(char? value) => this;
        public ILogEvent Append(short value) => this;
        public ILogEvent Append(short? value) => this;
        public ILogEvent Append(short value, string format) => this;
        public ILogEvent Append(short? value, string format) => this;
        public ILogEvent Append(int value) => this;
        public ILogEvent Append(int? value) => this;
        public ILogEvent Append(int value, string format) => this;
        public ILogEvent Append(int? value, string format) => this;
        public ILogEvent Append(long value) => this;
        public ILogEvent Append(long? value) => this;
        public ILogEvent Append(long value, string format) => this;
        public ILogEvent Append(long? value, string format) => this;
        public ILogEvent Append(float value) => this;
        public ILogEvent Append(float? value) => this;
        public ILogEvent Append(float value, string format) => this;
        public ILogEvent Append(float? value, string format) => this;
        public ILogEvent Append(double value) => this;
        public ILogEvent Append(double? value) => this;
        public ILogEvent Append(double value, string format) => this;
        public ILogEvent Append(double? value, string format) => this;
        public ILogEvent Append(decimal value) => this;
        public ILogEvent Append(decimal? value) => this;
        public ILogEvent Append(decimal value, string format) => this;
        public ILogEvent Append(decimal? value, string format) => this;
        public ILogEvent Append(Guid value) => this;
        public ILogEvent Append(Guid? value) => this;
        public ILogEvent Append(Guid value, string format) => this;
        public ILogEvent Append(Guid? value, string format) => this;
        public ILogEvent Append(DateTime value) => this;
        public ILogEvent Append(DateTime? value) => this;
        public ILogEvent Append(DateTime value, string format) => this;
        public ILogEvent Append(DateTime? value, string format) => this;
        public ILogEvent Append(TimeSpan value) => this;
        public ILogEvent Append(TimeSpan? value) => this;
        public ILogEvent Append(TimeSpan value, string format) => this;
        public ILogEvent Append(TimeSpan? value, string format) => this;
    }
}
