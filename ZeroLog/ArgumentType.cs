namespace ZeroLog
{
    enum ArgumentType : byte
    {
        String,
        Char,
        Byte,
        Boolean,
        Int8,
        Int16,
        Int32,
        Int64,
        Single,
        Float,
        Double,
        // TODO: Implement custom formatters for this
        Guid,
        DateTime,
        TimeSpan
    }
}