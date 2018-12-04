namespace ZeroLog
{
    internal unsafe class UnpooledLogEvent : LogEvent
    {
        public UnpooledLogEvent(BufferSegment bufferSegment, int argCapacity)
            : base(bufferSegment, argCapacity)
        {
        }

        public override bool IsPooled => false;

        public override string ToString()
        {
            return $"buffer length: {_endOfBuffer - _startOfBuffer}, data length: {_dataPointer - _startOfBuffer}, first arg type: {(ArgumentType)(*_startOfBuffer)}";
        }
    }
}
