using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Formatting;
using ZeroLog.Utils;

namespace ZeroLog
{
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    internal struct EnumArg
    {
        private IntPtr _typeHandle;
        private ulong _value;

        public EnumArg(IntPtr typeHandle, ulong value)
        {
            _typeHandle = typeHandle;
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendTo(StringBuffer stringBuffer)
        {
            var enumString = GetString();

            if (enumString != null)
                stringBuffer.Append(enumString);
            else
                AppendNumericValue(stringBuffer);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void AppendNumericValue(StringBuffer stringBuffer)
        {
            if (_value <= long.MaxValue)
            {
                stringBuffer.Append(_value, StringView.Empty);
                return;
            }

            if (EnumCache.IsEnumSigned(_typeHandle))
                stringBuffer.Append(unchecked((long)_value), StringView.Empty);
            else
                stringBuffer.Append(_value, StringView.Empty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? GetString()
            => EnumCache.GetString(_typeHandle, _value, out var enumRegistered)
               ?? GetStringSlow(enumRegistered);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private string? GetStringSlow(bool enumRegistered)
        {
            if (enumRegistered || !LogManager.Config.LazyRegisterEnums)
                return null;

            var type = TypeUtil.GetTypeFromHandle(_typeHandle);
            if (type is null)
                return null;

            LogManager.RegisterEnum(type);
            return EnumCache.GetString(_typeHandle, _value, out _);
        }
    }
}
