#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Managed.Networking.Lwip;

namespace Managed.Networking;

/// <summary>Owner-bound application callbacks. Protocol state machines remain translated.</summary>
public static unsafe class LwipUdpApplications
{
    sealed class State
    {
        internal object? SntpToken,TftpToken;
        internal Action<uint,uint>? Time;
        internal readonly List<Exception> Errors=new();
    }
    static readonly ConditionalWeakTable<Lwip,State> States=new();
    static State For(Lwip owner)=>States.GetValue(owner,static _=>new());
    public static void RegisterSntp(Lwip owner,object token,Action<uint,uint> notification)
    {
        ArgumentNullException.ThrowIfNull(token);ArgumentNullException.ThrowIfNull(notification);
        var state=For(owner);lock(state)
        {
            if(state.SntpToken!=null)throw new InvalidOperationException("This stack already owns an SNTP client.");
            state.SntpToken=token;state.Time=notification;
        }
    }
    public static void UnregisterSntp(Lwip owner,object token)
    {
        var state=For(owner);lock(state)
        {
            if(!ReferenceEquals(state.SntpToken,token))throw new InvalidOperationException("SNTP registration belongs to another service.");
            state.Time=null;state.SntpToken=null;
        }
    }
    /// <summary>Typed C macro boundary; words preserve the NTP era and 2^-32 second fraction. Does not set the OS clock.</summary>
    public static void SntpTime(Lwip owner,uint seconds,uint fraction)
    {
        if(!States.TryGetValue(owner,out var state))return;
        Action<uint,uint>? callback;lock(state)callback=state.Time;
        try{callback?.Invoke(seconds,fraction);}
        catch(Exception error){lock(state)state.Errors.Add(error);}
    }
    public static void ClaimTftp(Lwip owner,object token)
    {
        ArgumentNullException.ThrowIfNull(token);var state=For(owner);lock(state)
        {
            if(state.TftpToken!=null)throw new InvalidOperationException("This stack already owns a TFTP service.");
            state.TftpToken=token;
        }
    }
    public static void ReleaseTftp(Lwip owner,object token)
    {
        var state=For(owner);lock(state)
        {
            if(!ReferenceEquals(state.TftpToken,token))throw new InvalidOperationException("TFTP registration belongs to another service.");
            state.TftpToken=null;
        }
    }
    public static void ThrowErrors(Lwip owner)
    {
        var state=For(owner);lock(state)
        {
            if(state.Errors.Count==0)return;
            var errors=state.Errors.ToArray();state.Errors.Clear();throw new AggregateException("UDP application callback failed.",errors);
        }
    }
    /// <summary>Typed override of the original tiny PUT entry; correction runs before serialization/sending.</summary>
    public static sbyte TftpPut(Lwip owner,void* handle,ip_addr* address,ushort port,byte* filename,tftp_transfer_mode mode)
        =>owner.lwip_host_tftp_put(handle,address,port,filename,mode);
}
