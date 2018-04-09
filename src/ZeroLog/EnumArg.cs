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
            var enumString = EnumCache.GetString(_typeHandle, _value, out var enumRegistered);
            if (enumString != null)
            {
                stringBuffer.Append(enumString);
                return;
            }

            AppendToSlow(stringBuffer, enumRegistered);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AppendToSlow(StringBuffer stringBuffer, bool enumRegistered)
        {
            if (!enumRegistered && LogManager.Config.LazyRegisterEnums)
            {
                LogManager.RegisterEnum(TypeUtil.GetTypeFromHandle(_typeHandle));
                var enumString = EnumCache.GetString(_typeHandle, _value, out _);
                if (enumString != null)
                {
                    stringBuffer.Append(enumString);
                    return;
                }
            }

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
    }
}
