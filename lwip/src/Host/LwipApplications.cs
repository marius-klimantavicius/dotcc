#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Managed.Networking;

/// <summary>Application content and storage callbacks; all HTTP protocol handling stays translated.</summary>
public static unsafe class LwipApplications
{
    private sealed class Post(string path, int expected)
    {
        internal readonly string Path = path;
        internal readonly int Expected = expected;
        internal readonly MemoryStream Body = new();
    }
    private sealed class State
    {
        internal readonly Dictionary<string, byte[]> Content = new(StringComparer.Ordinal);
        internal readonly HashSet<nint> OpenFiles = new();
        internal readonly Dictionary<nint, Post> Posts = new();
        internal Func<string, byte[], string>? PostHandler;
        internal readonly Queue<Exception> Errors = new();
    }
    private static readonly ConditionalWeakTable<Lwip, State> States = new();
    private static State For(Lwip owner) => States.GetValue(owner, static _ => new());
    public static void AddContent(Lwip owner, string path, ReadOnlySpan<byte> data) { var state=For(owner);lock(state)state.Content[path]=data.ToArray(); }
    public static void SetPostHandler(Lwip owner, Func<string, byte[], string> handler) { var state=For(owner);lock(state)state.PostHandler=handler; }
    public static byte* HttpOpen(Lwip owner, byte* name, int* length)
    {
        var state=For(owner);lock(state)
        {
            if(!state.Content.TryGetValue(Marshal.PtrToStringUTF8((nint)name)!,out var content))return null;
            byte* data=null;
            try{data=(byte*)NativeMemory.Alloc((nuint)Math.Max(1,content.Length));content.AsSpan().CopyTo(new Span<byte>(data,content.Length));state.OpenFiles.Add((nint)data);*length=content.Length;return data;}
            catch(OutOfMemoryException error){NativeMemory.Free(data);state.Errors.Enqueue(error);return null;}
        }
    }
    public static void HttpClose(Lwip owner, byte* data)
    {
        var state=For(owner);lock(state)
        { if(!state.OpenFiles.Remove((nint)data))throw new InvalidOperationException("Closing unknown HTTP content.");NativeMemory.Free(data); }
    }
    public static int PostBegin(Lwip owner, void* connection, byte* uri, int length)
    {
        var state=For(owner);lock(state)
        {
            if(state.PostHandler==null || length<0 || length>16*1024*1024 || state.Posts.ContainsKey((nint)connection))return 0;
            state.Posts.Add((nint)connection,new Post(Marshal.PtrToStringUTF8((nint)uri)!,length));return 1;
        }
    }
    public static int PostData(Lwip owner, void* connection, byte* data, uint length)
    {
        var state=For(owner);lock(state)
        {
            if(!state.Posts.TryGetValue((nint)connection,out var post)||post.Body.Length+length>post.Expected)return 0;
            post.Body.Write(new ReadOnlySpan<byte>(data,checked((int)length)));return 1;
        }
    }
    public static void PostFinish(Lwip owner, void* connection, byte* response, uint capacity)
    {
        var state=For(owner);Post? post;Func<string,byte[],string>? handler;
        lock(state){state.Posts.Remove((nint)connection,out post);handler=state.PostHandler;}
        if(capacity!=0)*response=0;
        if(post==null)return;
        using(post.Body)
        {
            if(post.Body.Length!=post.Expected||handler==null)return;
            try
            {
                byte[] result=Encoding.UTF8.GetBytes(handler(post.Path,post.Body.ToArray()));
                if(result.Length>=capacity)throw new InvalidOperationException("HTTP POST response URI exceeds upstream buffer.");
                result.CopyTo(new Span<byte>(response,result.Length));response[result.Length]=0;
            }
            catch(Exception error){lock(state)state.Errors.Enqueue(error);}
        }
    }
    public static (int OpenFiles,int Posts) Counters(Lwip owner) { var state=For(owner);lock(state)return(state.OpenFiles.Count,state.Posts.Count); }
    public static void ThrowErrors(Lwip owner)
    {var state=For(owner);lock(state){if(state.Errors.Count==0)return;var errors=state.Errors.ToArray();state.Errors.Clear();throw new AggregateException("HTTP application callback failed.",errors);}}
    public static void Release(Lwip owner)
    {
        var state=For(owner);lock(state)
        {
            if(state.OpenFiles.Count!=0||state.Posts.Count!=0)throw new InvalidOperationException("HTTP files or POST requests still active; stop applications before disposing the stack.");
            state.Content.Clear();state.PostHandler=null;
        }
    }
}
