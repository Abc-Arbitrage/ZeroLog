using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    [TestFixture]
    public unsafe class BufferSegmentProviderTests
    {
        private BufferSegmentProvider _bufferSegmentProvider;
        private int _defaultSegmentSize = 8;

        [SetUp]
        public void SetUp()
        {
            _bufferSegmentProvider = new BufferSegmentProvider(32, _defaultSegmentSize);
        }

        [Test]
        public void should_get_buffer_segment_when_there_are_segments_available()
        {
            var bufferSegment = _bufferSegmentProvider.GetSegment();

            Check.That(bufferSegment.Length).Equals(_defaultSegmentSize);
            Check.That(new IntPtr(bufferSegment.Data)).IsNotEqualTo(IntPtr.Zero);

            Check.That(_bufferSegmentProvider.LargeBufferCount).Equals(1);
            Check.That(_bufferSegmentProvider.LastSegmentIndex).Equals(0);
        }

        [Test]
        public void should_get_all_segments_from_a_large_buffer()
        {
            var segments = new List<BufferSegment>();
            for (var i = 0; i < 4; i++)
            {
                segments.Add(_bufferSegmentProvider.GetSegment());
            }

            var lastAddress = (int)segments[0].Data;
            for (var i = 1; i < segments.Count; i++)
            {
                var bufferSegment = segments[i];
                Check.That((int)bufferSegment.Data).IsEqualTo(lastAddress + _defaultSegmentSize);
                Check.That(bufferSegment.Length).Equals(_defaultSegmentSize);
                lastAddress = (int)bufferSegment.Data;
            }

            Check.That(_bufferSegmentProvider.LargeBufferCount).Equals(1);
            Check.That(_bufferSegmentProvider.LastSegmentIndex).Equals(3);
        }

        [Test]
        public void should_allocate_a_new_large_buffer_when_needed()
        {
            var segments = new List<BufferSegment>();
            for (var i = 0; i < 5; i++)
            {
                segments.Add(_bufferSegmentProvider.GetSegment());
            }

            Check.That(_bufferSegmentProvider.LargeBufferCount).Equals(2);
            Check.That(_bufferSegmentProvider.LastSegmentIndex).Equals(4);
        }
    }
}
