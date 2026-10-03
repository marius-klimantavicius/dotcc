#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Edg.Runtime;

/// <summary>One initial environment snapshot per translated engine. Mutation,
/// lookup and putenv ownership belong to the translated musl implementation;
/// this boundary never modifies the .NET host's process environment.</summary>
public sealed unsafe class LibcEnvironment : IDisposable
{
    private static readonly Encoding utf8=new UTF8Encoding(false,true);
    private byte[]? strings;
    private nint[]? entries;
    private bool disposed;

    public LibcEnvironment() : this(HostEntries()) { }

    // Explicit initial data also supports deterministic hosts and synthetic
    // verification without altering the surrounding process environment.
    public LibcEnvironment(IEnumerable<KeyValuePair<string,string>> initial)
    {
        ArgumentNullException.ThrowIfNull(initial);
        var values=new List<KeyValuePair<string,string>>(initial);
        int length=0;
        foreach(var entry in values) {
            ArgumentNullException.ThrowIfNull(entry.Key);
            ArgumentNullException.ThrowIfNull(entry.Value);
            if(entry.Key.IndexOf('\0')>=0 || entry.Value.IndexOf('\0')>=0)
                throw new ArgumentException("Environment entries cannot contain NUL",nameof(initial));
            length=checked(length+utf8.GetByteCount(entry.Key)+utf8.GetByteCount(entry.Value)+2);
        }
        // Environment strings and pointer vectors are addressable global state.
        // Their addresses stay fixed until the translated owner is disposed.
        strings=GC.AllocateArray<byte>(Math.Max(length,1),pinned:true);
        entries=GC.AllocateArray<nint>(checked(values.Count+1),pinned:true);
        fixed(byte* arena=strings) {
            int offset=0,index=0;
            foreach(var entry in values) {
                entries[index++]=(nint)(arena+offset);
                offset+=utf8.GetBytes(entry.Key,strings.AsSpan(offset));
                strings[offset++]=(byte)'=';
                offset+=utf8.GetBytes(entry.Value,strings.AsSpan(offset));
                strings[offset++]=0;
            }
        }
    }
    private static IEnumerable<KeyValuePair<string,string>> HostEntries()
    {
        foreach(DictionaryEntry entry in Environment.GetEnvironmentVariables())
            yield return new KeyValuePair<string,string>((string)entry.Key,(string)entry.Value!);
    }
    public sbyte** Snapshot()
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        fixed(nint* table=entries!)return (sbyte**)table;
    }
    public void Dispose()
    {
        if(disposed)return;
        disposed=true;entries=null;strings=null;
    }
}
