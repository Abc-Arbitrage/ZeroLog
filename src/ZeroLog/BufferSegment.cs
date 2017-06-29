namespace ZeroLog
{
    internal unsafe struct BufferSegment
    {
        public readonly byte* Data;
        public readonly int Length;

        public BufferSegment(byte* data, int length)
        {
            Data = data;
            Length = length;
        }
    }
}
