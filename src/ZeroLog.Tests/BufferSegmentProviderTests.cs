using System;
using System.Collections.Generic;
using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public unsafe class BufferSegmentProviderTests
{
    private BufferSegmentProvider _bufferSegmentProvider;

    private const int _segmentCount = 4;
    private const int _segmentSize = 8;

    [SetUp]
    public void SetUp()
    {
        _bufferSegmentProvider = new BufferSegmentProvider(_segmentCount, _segmentSize);
    }

    [Test]
    public void should_get_buffer_segment_when_there_are_segments_available()
    {
        var bufferSegment = _bufferSegmentProvider.GetSegment();

        bufferSegment.Length.ShouldEqual(_segmentSize);
        new IntPtr(bufferSegment.Data).ShouldNotEqual(IntPtr.Zero);
        bufferSegment.UnderlyingBuffer.ShouldNotBeNull();
    }

    [Test]
    public void should_get_all_segments_from_a_large_buffer()
    {
        var segments = new List<BufferSegment>();

        for (var i = 0; i < _segmentCount; ++i)
            segments.Add(_bufferSegmentProvider.GetSegment());

        var lastAddress = (nuint)segments[0].Data;

        for (var i = 1; i < segments.Count; i++)
        {
            var bufferSegment = segments[i];
            ((nuint)bufferSegment.Data).ShouldEqual(lastAddress + _segmentSize);
            bufferSegment.Length.ShouldEqual(_segmentSize);
            lastAddress = (nuint)bufferSegment.Data;
        }
    }

    [Test]
    public void should_allocate_a_new_large_buffer_when_needed()
    {
        var segments = new List<BufferSegment>();

        for (var i = 0; i < 2 * _segmentCount; i++)
            segments.Add(_bufferSegmentProvider.GetSegment());

        segments[_segmentCount - 1].UnderlyingBuffer.ShouldBeTheSameAs(segments[0].UnderlyingBuffer);
        segments[_segmentCount].UnderlyingBuffer.ShouldNotBeTheSameAs(segments[_segmentCount - 1].UnderlyingBuffer);
        segments[_segmentCount + 1].UnderlyingBuffer.ShouldBeTheSameAs(segments[_segmentCount].UnderlyingBuffer);

        ((nuint)segments[_segmentCount + 1].Data).ShouldEqual((nuint)segments[_segmentCount].Data + _segmentSize);
    }

    [Test]
    public void should_validate_arguments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new BufferSegmentProvider(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new BufferSegmentProvider(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new BufferSegmentProvider(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new BufferSegmentProvider(1, -1));
    }

    [Test]
    public void should_limit_buffer_size()
    {
        var provider = new BufferSegmentProvider(4 * 1024, 1024 * 1024);
        provider.BufferSize.ShouldEqual(1024 * 1024 * 1024);
    }
}
