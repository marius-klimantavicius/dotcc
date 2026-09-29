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
        private readonly Queue<Exception> errors=new();
        private readonly int capacity;
        private bool aborted;
        public SerialChannel(Action<byte[]> output,int capacity=65536)
        {this.output=output??throw new ArgumentNullException(nameof(output));if(capacity<=0)throw new ArgumentOutOfRangeException(nameof(capacity));this.capacity=capacity;}
        public int PendingBytes { get { lock(gate)return input.Count; } }
        /// <summary>Atomically admit every byte or return false without changing the queue.</summary>
        public bool TryFeed(ReadOnlySpan<byte> bytes)
        {lock(gate){if(bytes.Length>capacity-input.Count)return false;foreach(byte value in bytes)input.Enqueue(value);Monitor.PulseAll(gate);return true;}}
        public void ThrowErrors()
        {lock(gate){if(errors.Count==0)return;var failures=errors.ToArray();errors.Clear();throw new AggregateException("Serial output callback failed.",failures);}}
        public void Feed(ReadOnlySpan<byte> bytes)
        {
            if(!TryFeed(bytes))throw new InvalidOperationException("Bounded serial input queue is full.");
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
        internal uint Write(byte* bytes, uint length) {try{output(new ReadOnlySpan<byte>(bytes,checked((int)length)).ToArray());return length;}catch(Exception error){lock(gate)errors.Enqueue(error);return 0;}}
        internal void Abort() { lock (gate) { aborted = true; Monitor.PulseAll(gate); } }
    }
    public static void AttachSerial(Lwip owner, byte device, SerialChannel channel)
    {
        var state = For(owner); lock (state.Registry)
        { if (state.Stopping) throw new Stopped(); state.Serial.Add(device, channel);long handle=Interlocked.Increment(ref nextHandle);state.SerialHandles.Add(device,handle);state.SerialByHandle.Add(handle,channel); }
    }
    private static SerialChannel Serial(Lwip owner, void* handle)
    {
        var state = For(owner); lock (state.Registry)
        {
            return state.SerialByHandle.TryGetValue((long)handle,out var channel)?channel:throw new InvalidOperationException("Invalid or foreign serial transport handle.");
        }
    }
    public static void* SerialOpen(Lwip owner, byte device)
    { var state = For(owner); lock (state.Registry) return state.SerialHandles.TryGetValue(device,out var handle) ? (void*)handle : null; }
    public static void SerialSend(Lwip owner, byte value, void* handle) => Serial(owner, handle).Write(&value, 1);
    public static byte SerialReceive(Lwip owner, void* handle)
    { byte value = 0; if (SerialRead(owner, handle, &value, 1) == 0) throw new OperationCanceledException("Serial read aborted."); return value; }
    /// <summary>Only a specifically requested ancillary service may exit here. Caller must join it before releasing partial decoder state.</summary>
    public static uint SerialRead(Lwip owner, void* handle, byte* bytes, uint length)
    {ExitServiceIfRequested(owner);uint count=Serial(owner,handle).Read(For(owner),bytes,length,true);ExitServiceIfRequested(owner);return count;}
    public static uint SerialTryRead(Lwip owner, void* handle, byte* bytes, uint length) => Serial(owner, handle).Read(For(owner), bytes, length, false);
    public static uint SerialWrite(Lwip owner, void* handle, byte* bytes, uint length) => Serial(owner, handle).Write(bytes, length);
    public static void SerialAbort(Lwip owner, void* handle) => Serial(owner, handle).Abort();
}
