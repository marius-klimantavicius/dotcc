using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
using static UdpPeer;

internal static unsafe class TftpCases
{
    private sealed class FileState(Storage storage, string name, byte[] initial, bool write)
    {
        internal readonly Storage Storage = storage;
        internal readonly string Name = name;
        internal readonly bool Write = write;
        internal readonly MemoryStream Data = write ? new MemoryStream() : new MemoryStream(initial, false);
    }

    private sealed class Storage
    {
        internal readonly Dictionary<string, byte[]> Files = new(StringComparer.Ordinal);
        internal readonly HashSet<nint> Handles = new();
        internal int Opens, Closes;
        internal Exception? Error;
    }
    private static readonly ConcurrentDictionary<Lwip, Storage> Stores = new();
    private static FileState File(void* handle) => (FileState)GCHandle.FromIntPtr((nint)handle).Target!;

    private static void* Open(Lwip owner, byte* name, byte* mode, byte write)
    {
        var store = Stores[owner];
        try
        {
            Require(Marshal.PtrToStringUTF8((nint)mode) == "octet", "TFTP octet storage mode");
            string path = Marshal.PtrToStringUTF8((nint)name)!;
            if (write == 0 && !store.Files.ContainsKey(path)) return null;
            var file = new FileState(store, path, write == 0 ? store.Files[path] : [], write != 0);
            nint handle = GCHandle.ToIntPtr(GCHandle.Alloc(file));
            store.Handles.Add(handle); store.Opens++;
            return (void*)handle;
        }
        catch (Exception failure) { store.Error = failure; return null; }
    }
    private static void Close(Lwip owner, void* handle)
    {
        // Capture ordinary callback failures without unwinding through translated C.
        if (!Stores.TryGetValue(owner, out var store)) return;
        FileState? file = null;
        try
        {
            Require(store.Handles.Contains((nint)handle), "TFTP closes a registered handle");
            file = File(handle);
            Require(ReferenceEquals(Stores[owner], store), "TFTP close owner");
            Require(store.Handles.Remove((nint)handle), "TFTP handle closes once");
            if (file.Write) store.Files[file.Name] = file.Data.ToArray();
            store.Closes++;
        }
        catch (Exception failure) { store.Error = failure; }
        finally
        {
            if (file != null)
            {
                try { file.Data.Dispose(); GCHandle.FromIntPtr((nint)handle).Free(); }
                catch (Exception failure) { store.Error ??= failure; }
            }
        }
    }
    private static int Read(Lwip owner, void* handle, void* bytes, int count)
    {
        var file = File(handle);
        try
        {
            Require(ReferenceEquals(Stores[owner], file.Storage) && !file.Write, "TFTP read owner/mode");
            return file.Data.Read(new Span<byte>(bytes, count));
        }
        catch (Exception failure) { file.Storage.Error = failure; return -1; }
    }
    private static int Write(Lwip owner, void* handle, pbuf* packet)
    {
        var file = File(handle);
        try
        {
            Require(ReferenceEquals(Stores[owner], file.Storage) && file.Write, "TFTP write owner/mode");
            Require(file.Data.Length + packet->tot_len <= 4096, "bounded fixture storage");
            for (var part = packet; part != null; part = part->next)
                file.Data.Write(new ReadOnlySpan<byte>(part->payload, part->len));
            return 0;
        }
        catch (Exception failure) { file.Storage.Error = failure; return -1; }
        // Original tftp_recv owns and frees this pbuf after the callback.
    }
    private static void Error(Lwip owner, void* handle, int code, byte* message, int length)
    {
        if (!Stores.TryGetValue(owner, out var store)) return;
        try
        {
            Require(store.Handles.Contains((nint)handle), "TFTP error references a live handle");
            store.Error = new InvalidOperationException($"Unexpected TFTP error {code}: {Encoding.ASCII.GetString(new ReadOnlySpan<byte>(message, length))}");
        }
        catch (Exception failure) { store.Error ??= failure; }
    }

    private static byte[] Request(ushort opcode, string name)
    {
        byte[] body = Encoding.ASCII.GetBytes(name + "\0octet\0"), packet = new byte[2 + body.Length];
        BinaryPrimitives.WriteUInt16BigEndian(packet, opcode); body.CopyTo(packet, 2); return packet;
    }
    private static byte[] Block(ushort opcode, ushort block, ReadOnlySpan<byte> payload = default)
    {
        byte[] packet = new byte[4 + payload.Length];
        BinaryPrimitives.WriteUInt16BigEndian(packet, opcode);
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(2), block); payload.CopyTo(packet.AsSpan(4)); return packet;
    }
    private static void Header(byte[] bytes, ushort opcode, ushort block)
    { Require(bytes.Length >= 4 && BinaryPrimitives.ReadUInt16BigEndian(bytes) == opcode && BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(2)) == block, "TFTP opcode/block"); }

    internal static void ConstructionRollback(VirtualPair pair)
    {
        var store=new Storage();Stores[pair.A.Core]=store;bool started=false;
        tftp_context* context=(tftp_context*)NativeMemory.AllocZeroed((nuint)sizeof(tftp_context));
        context->open=&Open;context->close=&Close;context->read=&Read;context->write=&Write;context->error=&Error;
        using var peer=new UdpPeer(pair.B,31069);
        try
        {
            pair.A.Invoke(owner=>{Require(owner.tftp_init_client(context)==0,"borrowed original client starts");started=true;});
            bool rejected=false;
            try{using var unexpected=LwipTftpService.StartClient(pair.A);}
            catch(IOException error){rejected=error.Message.Contains($"({(int)err_enum_t.ERR_USE})",StringComparison.Ordinal);}
            Require(rejected,"owning start reaches original ERR_USE after allocating its context");
            pair.A.Invoke(owner=>
            {
                ip_addr destination;fixed(byte* address="10.0.0.2\0"u8)Require(owner.ipaddr_aton(address,&destination)==1,"borrowed destination");
                fixed(byte* local="local.bin\0"u8,mode="octet\0"u8,remote="remote.bin\0"u8)
                {void* handle=Open(owner,local,mode,1);Require(handle!=null&&owner.tftp_get(handle,&destination,31069,remote,tftp_transfer_mode.TFTP_MODE_OCTET)==0,"borrowed client survives failed owning start");}
            });
            var request=peer.Next(pair);Require(request.Bytes.AsSpan().SequenceEqual(Request(1,"remote.bin")),"borrowed original RRQ unchanged");
            byte[] expected="borrowed-is-alive"u8.ToArray();peer.Send("10.0.0.1",request.Port,Block(3,1,expected));
            var ack=peer.Next(pair);Header(ack.Bytes,4,1);Require(ack.Bytes.Length==4,"borrowed original final ACK");pair.Drain();
            pair.A.Invoke(owner=>{if(store.Error!=null)throw store.Error;Require(store.Closes==1&&store.Handles.Count==0&&store.Files["local.bin"].AsSpan().SequenceEqual(expected),"borrowed storage remains intact");owner.tftp_cleanup();started=false;});
            using(var recovered=LwipTftpService.StartClient(pair.A))Require(recovered.ActiveTransfers==0,"failed owning claim and callback registration released; owning restart succeeds");
        }
        finally
        {
            if(started)pair.A.Invoke(owner=>owner.tftp_cleanup());pair.Drain();Stores.TryRemove(pair.A.Core,out _);NativeMemory.Free(context);
            Require(store.Handles.Count==0,"borrowed rollback fixture releases all storage roots");if(store.Error!=null)throw store.Error;
        }
    }

    internal static void Run(VirtualPair pair)
    {
        var store = new Storage(); Stores[pair.B.Core] = store;
        tftp_context* context = (tftp_context*)NativeMemory.AllocZeroed((nuint)sizeof(tftp_context));
        context->open = &Open; context->close = &Close; context->read = &Read; context->write = &Write; context->error = &Error;
        bool initialized = false;
        using var peer = new UdpPeer(pair.A, 31069);
        try
        {
            pair.B.Invoke(owner => { Require(owner.tftp_init_server(context) == 0, "TFTP server start"); initialized = true; });
            foreach (int length in new[] { 0, 512, 1025 })
            {
                byte[] expected = Enumerable.Range(0, length).Select(i => (byte)(i % 251)).ToArray();
                string name = $"read-{length}.bin";
                pair.B.Invoke(_ => store.Files[name] = expected);
                peer.Send("10.0.0.2", 69, Request(1, name));
                using var result = new MemoryStream();
                for (ushort block = 1; ; block++)
                {
                    var data = peer.Next(pair); Header(data.Bytes, 3, block);
                    Require(data.Bytes.Length <= 516, "TFTP data block bound");
                    result.Write(data.Bytes.AsSpan(4)); peer.Send("10.0.0.2", data.Port, Block(4, block));
                    if (data.Bytes.Length < 516) break;
                }
                pair.Drain(); Require(result.ToArray().SequenceEqual(expected), "TFTP server read bytes");
                pair.B.Invoke(_ => { if (store.Error != null) throw store.Error; Require(store.Handles.Count == 0, "TFTP read releases handle"); });

                name = $"write-{length}.bin"; peer.Send("10.0.0.2", 69, Request(2, name));
                var ack = peer.Next(pair); Header(ack.Bytes, 4, 0); ushort serverPort = ack.Port;
                int offset = 0;
                for (ushort block = 1; ; block++)
                {
                    int count = Math.Min(512, expected.Length - offset);
                    peer.Send("10.0.0.2", serverPort, Block(3, block, expected.AsSpan(offset, count)));
                    ack = peer.Next(pair); Header(ack.Bytes, 4, block); Require(ack.Bytes.Length == 4, "TFTP ACK size");
                    offset += count; if (count < 512) break;
                }
                pair.Drain();
                pair.B.Invoke(_ => { if (store.Error != null) throw store.Error; Require(store.Files[name].SequenceEqual(expected), "TFTP server write bytes"); Require(store.Handles.Count == 0, "TFTP write releases handle"); });
            }
            pair.B.Invoke(_ => Require(store.Opens == 6 && store.Closes == 6, "six TFTP transfers close storage exactly once"));
        }
        finally
        {
            if (initialized) pair.B.Invoke(owner => owner.tftp_cleanup());
            // Keep callbacks and context rooted through original cleanup and its close callback.
            pair.Drain(); Stores.TryRemove(pair.B.Core, out _); NativeMemory.Free(context);
            Require(store.Handles.Count == 0, "TFTP cleanup leaves no storage handles");
            if (store.Error != null) throw store.Error;
        }
    }

    internal static void RunClientGet(VirtualPair pair) => ClientTransfer(pair, false);

    // Positive product PUT uses the guarded typed entry adapter. The original
    // block-one failure and matching native block-zero pass remain preserved.
    internal static void RunClientPut(VirtualPair pair) => ClientTransfer(pair, true);

    private static void ClientTransfer(VirtualPair pair, bool put)
    {
        var client = new Storage(); var server = new Storage();
        Stores[pair.A.Core] = client; Stores[pair.B.Core] = server;
        byte[] expected = Enumerable.Range(0, 1025).Select(i => (byte)(i % 251)).ToArray();
        if (put) client.Files["local.bin"] = expected; else server.Files["remote.bin"] = expected;
        tftp_context* context = (tftp_context*)NativeMemory.AllocZeroed((nuint)sizeof(tftp_context));
        context->open = &Open; context->close = &Close; context->read = &Read; context->write = &Write; context->error = &Error;
        bool clientStarted = false, serverStarted = false;
        try
        {
            pair.B.Invoke(owner => { Require(owner.tftp_init_server(context) == 0, "TFTP peer server start"); serverStarted = true; });
            pair.A.Invoke(owner =>
            {
                Require(owner.tftp_init_client(context) == 0, "original TFTP client start"); clientStarted = true;
                ip_addr destination;
                fixed (byte* address = Encoding.ASCII.GetBytes("10.0.0.2\0")) Require(owner.ipaddr_aton(address, &destination) == 1, "TFTP client destination");
                fixed (byte* local = Encoding.ASCII.GetBytes("local.bin\0"), mode = Encoding.ASCII.GetBytes("octet\0"), remote = Encoding.ASCII.GetBytes("remote.bin\0"))
                {
                    void* handle = Open(owner, local, mode, (byte)(put ? 0 : 1));
                    Require(handle != null, "TFTP client storage handle");
                    sbyte result = put ? owner.tftp_put(handle, &destination, 69, remote, tftp_transfer_mode.TFTP_MODE_OCTET)
                        : owner.tftp_get(handle, &destination, 69, remote, tftp_transfer_mode.TFTP_MODE_OCTET);
                    if (result != 0)
                    {
                        // Both original calls store handle before send_request;
                        // even synchronous send failure therefore transfers it.
                        // Original cleanup closes it; do not double-close here.
                        owner.tftp_cleanup(); clientStarted = false;
                        Require(client.Handles.Count == 0 && client.Closes == 1, "failed TFTP submission releases transferred storage through cleanup");
                        throw new InvalidOperationException("TFTP original client submission: " + result);
                    }
                }
            });
            Wait(pair, () =>
            {
                bool a = false, b = false;
                pair.A.Invoke(_ => { if (client.Error != null) throw client.Error; a = client.Closes == 1 && client.Handles.Count == 0; });
                pair.B.Invoke(_ => { if (server.Error != null) throw server.Error; b = server.Closes == 1 && server.Handles.Count == 0; });
                return a && b;
            });
            if (put) pair.B.Invoke(_ => Require(server.Files["remote.bin"].SequenceEqual(expected), "original TFTP PUT bytes"));
            else pair.A.Invoke(_ => Require(client.Files["local.bin"].SequenceEqual(expected), "original TFTP GET bytes"));
        }
        finally
        {
            if (clientStarted) pair.A.Invoke(owner => owner.tftp_cleanup());
            if (serverStarted) pair.B.Invoke(owner => owner.tftp_cleanup());
            pair.Drain(); Stores.TryRemove(pair.A.Core, out _); Stores.TryRemove(pair.B.Core, out _); NativeMemory.Free(context);
            Require(client.Handles.Count == 0 && server.Handles.Count == 0, "TFTP client/server cleanup handles");
        }
    }
}
