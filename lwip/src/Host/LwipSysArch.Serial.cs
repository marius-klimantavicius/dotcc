#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace Managed.Networking;

public static unsafe partial class LwipSysArch
{
    /// <summary>Managed byte transport. Output receives a copy; Feed transfers input bytes into an owned queue.</summary>
    public sealed class SerialChannel
    {
        private readonly object gate = new();
        private readonly Queue<byte> input = new();
        private readonly Action<byte[]> output;
        private bool aborted;
        public SerialChannel(Action<byte[]> output) => this.output = output ?? throw new ArgumentNullException(nameof(output));
        public void Feed(ReadOnlySpan<byte> bytes)
        {
            lock (gate) { foreach (byte value in bytes) input.Enqueue(value); Monitor.PulseAll(gate); }
        }
        internal uint Read(State state, byte* bytes, uint length, bool blocking)
        {
            if (length == 0) return 0;
            lock (gate)
            {
                while (input.Count == 0 && !aborted && !state.Stopping && blocking) Monitor.Wait(gate);
                if (state.Stopping) throw new Stopped();
                if (aborted) { aborted = false; return 0; }
                uint count = 0; while (count < length && input.Count != 0) bytes[count++] = input.Dequeue();
                return count;
            }
        }
        internal uint Write(byte* bytes, uint length) { output(new ReadOnlySpan<byte>(bytes, checked((int)length)).ToArray()); return length; }
        internal void Abort() { lock (gate) { aborted = true; Monitor.PulseAll(gate); } }
    }
    public static void AttachSerial(Lwip owner, byte device, SerialChannel channel)
    {
        var state = For(owner); lock (state.Registry)
        { if (state.Stopping) throw new Stopped(); state.Serial.Add(device, channel); }
    }
    private static SerialChannel Serial(Lwip owner, void* handle)
    {
        var state = For(owner); lock (state.Registry)
        {
            long id = (long)handle - 1;
            return id is >= 0 and <= 255 && state.Serial.TryGetValue((byte)id, out var channel) ? channel : throw new InvalidOperationException("Unknown serial transport.");
        }
    }
    public static void* SerialOpen(Lwip owner, byte device)
    { var state = For(owner); lock (state.Registry) return state.Serial.ContainsKey(device) ? (void*)(device + 1) : null; }
    public static void SerialSend(Lwip owner, byte value, void* handle) => Serial(owner, handle).Write(&value, 1);
    public static byte SerialReceive(Lwip owner, void* handle)
    { byte value = 0; if (SerialRead(owner, handle, &value, 1) == 0) throw new OperationCanceledException("Serial read aborted."); return value; }
    public static uint SerialRead(Lwip owner, void* handle, byte* bytes, uint length) => Serial(owner, handle).Read(For(owner), bytes, length, true);
    public static uint SerialTryRead(Lwip owner, void* handle, byte* bytes, uint length) => Serial(owner, handle).Read(For(owner), bytes, length, false);
    public static uint SerialWrite(Lwip owner, void* handle, byte* bytes, uint length) => Serial(owner, handle).Write(bytes, length);
    public static void SerialAbort(Lwip owner, void* handle) => Serial(owner, handle).Abort();
}
