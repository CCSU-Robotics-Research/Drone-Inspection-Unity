using System;
using System.Collections.Generic;
using System.Data.OleDb;
using NUnit.Framework;
using UnityEditor.VersionControl;

/// <summary>
/// Some unit tests for FrameAssembler. Bytes are built here
/// in the same way like the ground station. These tests confirm
/// behavior matches.
/// </summary>
public class FrameAssemblerTests
{
    private static byte[] Pack(byte[] payload)
    {
        var wire = new byte[4 + payload.Length];
        wire[0] = (byte)(payload.Length & 0xFF);
        wire[1] = (byte)((payload.Length >> 8) & 0xFF);
        wire[2] = (byte)((payload.Length >> 16) & 0xFF);
        wire[3] = (byte)((payload.Length >> 24) & 0xFF);
        Array.Copy(payload, 0, wire, 4, payload.Length);
        return wire;
    }

    [Test]
    public void SingleFrameSingleChunk()
    {
        byte[] wire = Pack(new byte[] { 1, 2, 3 });
        var payloads = new FrameAssembler()
            .ExtractPayloads(wire, wire.Length);

        Assert.AreEqual(1, payloads.Count);
        Assert.AreEqual(new byte[] { 1, 2, 3 }, payloads[0]);
    }

    [Test]
    public void FrameSentOneByteAtATime()
    {
        byte[] wire = Pack(new byte[] { 5, 6, 7, 8 });
        var assembler = new FrameAssembler();
        var got = new List<byte[]>();

        foreach (byte b in wire)
        {
            got.AddRange(assembler.ExtractPayloads(new[] { b }, 1));
        }

        Assert.AreEqual(1, got.Count);
        Assert.AreEqual(new byte[] { 5, 6, 7, 8 }, got[0]);
    }

    [Test]
    public void TwoFramesInOneChunk()
    {
        var wire = new List<byte>();
        wire.AddRange(Pack(new byte[] { 1 }));
        wire.AddRange(Pack(new byte[] { 2, 2 }));
        byte[] chunk = wire.ToArray();

        var payloads = new FrameAssembler()
            .ExtractPayloads(chunk, chunk.Length);

        Assert.AreEqual(2, payloads.Count);
        Assert.AreEqual(new byte[] { 1 }, payloads[0]);
        Assert.AreEqual(new byte[] { 2, 2 }, payloads[1]);
    }

    [Test]
    public void FrameSplitAcrossChunks()
    {
        byte[] wire = Pack(new byte[] { 9, 8, 7 });
        var first = new byte[5];
        var second = new byte[wire.Length - 5];
        Array.Copy(wire, 0, first, 0, 5);
        Array.Copy(wire, 5, second, 0, second.Length);

        var assembler = new FrameAssembler();
        Assert.AreEqual(
            0, assembler.ExtractPayloads(first, first.Length).Count);

        var payloads = assembler.ExtractPayloads(
            second, second.Length);
        Assert.AreEqual(1, payloads.Count);
        Assert.AreEqual(new byte[] { 9, 8, 7 }, payloads[0]);
    }

    [Test]
    public void OnlyCountBytes()
    {
        // Tests that anything past `count` is ignored
        byte[] wire = Pack(new byte[] { 4, 4 });
        var chunk = new byte[wire.Length + 8];
        Array.Copy(wire, chunk, wire.Length);
        for (int i = wire.Length; i < chunk.Length; i++)
        {
            chunk[i] = 0xFF;
        }

        var assembler = new FrameAssembler();
        var payloads = assembler.ExtractPayloads(chunk, wire.Length);

        Assert.AreEqual(1, payloads.Count);
        Assert.AreEqual(new byte[] { 4, 4 }, payloads[0]);
        Assert.AreEqual(
            0, assembler.ExtractPayloads(new byte[0], 0).Count);
    }

    [Test]
    public void ZeroLengthTest()
    {
        var zero = new byte[] { 0, 0, 0, 0 };
        Assert.Throws<InvalidOperationException>(
            () => new FrameAssembler().ExtractPayloads(zero, 4));
    }

    [Test]
    public void OversizeLengthTest()
    {
        uint bad = FrameAssembler.MaxFrameBytes + 1;
        var wire = new byte[]
        {
            (byte)(bad & 0xFF),
            (byte)((bad >> 8) & 0xFF),
            (byte)((bad >> 16) & 0xFF),
            (byte)((bad >> 24) & 0xFF),
        };
        Assert.Throws<InvalidOperationException>(
            () => new FrameAssembler().ExtractPayloads(wire, 4));
    }
}
