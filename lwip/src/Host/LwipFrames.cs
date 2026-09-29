#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Managed.Networking;

public static unsafe class LwipFrames
{
    private sealed class State
    {
        internal readonly Dictionary<nint, Action<byte[]>> Outputs = new();
        internal readonly Queue<Exception> Errors = new();
    }
    private static readonly ConditionalWeakTable<Lwip, State> States = new();
    private static State For(Lwip owner) => States.GetValue(owner,static _=>new());
    public static void Attach(Lwip owner,nint interfaceAddress,Action<byte[]> output)
    {var state=For(owner);lock(state)state.Outputs.Add(interfaceAddress,output);}
    public static void Detach(Lwip owner,nint interfaceAddress)
    {var state=For(owner);lock(state)state.Outputs.Remove(interfaceAddress);}
    public static int Send(Lwip owner,void* interfaceAddress,byte* bytes,uint length)
    {
        var state=For(owner);Action<byte[]>? output;
        lock(state)state.Outputs.TryGetValue((nint)interfaceAddress,out output);
        if(output==null)return -12; // ERR_IF, no configured link.
        try {output(new ReadOnlySpan<byte>(bytes,checked((int)length)).ToArray());return 0;}
        catch(Exception error){lock(state)state.Errors.Enqueue(error);return -12;}
    }
    public static void ThrowErrors(Lwip owner)
    {var state=For(owner);lock(state){if(state.Errors.Count==0)return;var errors=state.Errors.ToArray();state.Errors.Clear();throw new AggregateException("Managed frame output failed.",errors);}}
}
