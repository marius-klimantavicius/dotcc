using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Managed.Networking.Lwip;

namespace Managed.Networking;

// Owner-bound facade over original MQTT; wire behavior remains in translated C.
public enum LwipMqttConnectionStatus
{
    Accepted=0,RefusedProtocolVersion=1,RefusedIdentifier=2,RefusedServer=3,
    RefusedCredentials=4,RefusedAuthorization=5,Disconnected=256,Timeout=257
}
public readonly record struct LwipMqttConnectionEvent(LwipMqttConnectionStatus Status,bool Local);
public sealed record LwipMqttPublication(string Topic,ReadOnlyMemory<byte> Payload);
public sealed record LwipMqttOptions
{
    public int MaximumPendingRequests{get;init;}=4;
    public int PublicationQueueCapacity{get;init;}=32;
    public int ConnectionEventQueueCapacity{get;init;}=16;
    public int MaximumPublicationBytes{get;init;}=65536;
    public int MaximumTopicBytes{get;init;}=1024;
    public int MaximumOutgoingPayloadBytes{get;init;}=65535;
}
public sealed record LwipMqttConnectOptions(string ClientId)
{
    public string? UserName{get;init;}
    public string? Password{get;init;}
    public ushort KeepAliveSeconds{get;init;}=60;
    public string? WillTopic{get;init;}
    public ReadOnlyMemory<byte> WillPayload{get;init;}
    public byte WillQualityOfService{get;init;}
    public bool WillRetain{get;init;}
}
public sealed class LwipMqttException(string operation,int error):IOException($"Original MQTT {operation} failed ({error}).")
{public int Error{get;}=error;}
public sealed class LwipMqttDisconnectedException(LwipMqttConnectionStatus status):IOException($"Original MQTT connection ended ({status}).")
{public LwipMqttConnectionStatus Status{get;}=status;}

/// <summary>Owns one original MQTT client. Multiple clients may share a stack. Notification queues are bounded; full queues discard new items. No MQTT protocol is implemented in this wrapper.</summary>
public sealed unsafe class LwipMqttClient:IDisposable
{
    sealed class Registry
    {
        internal readonly Dictionary<nint,LwipMqttClient> Clients=new();
        internal readonly Dictionary<nint,Request> Requests=new();
    }
    sealed class Request(LwipMqttClient service,string operation)
    {
        internal readonly LwipMqttClient Service=service;
        internal readonly string Operation=operation;
        internal readonly TaskCompletionSource Completion=new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal GCHandle Root;
    }
    sealed class Incoming(string topic,int length)
    {
        internal readonly string Topic=topic;
        internal readonly byte[] Bytes=new byte[length];
        internal int Position;
    }
    static readonly ConditionalWeakTable<Lwip,Registry> Registries=new();
    static readonly UTF8Encoding Utf8=new(false,true);
    readonly LwipStack stack;
    readonly LwipMqttOptions options;
    readonly Channel<LwipMqttPublication> publications;
    readonly Channel<LwipMqttConnectionEvent> connections;
    readonly Dictionary<nint,Request> pending=new();
    Registry? registry;
    GCHandle root;
    nint token,client;
    bool registered,connected,closing,stopped;
    volatile bool disposed;
    TaskCompletionSource<LwipMqttConnectionStatus>? connecting;
    Incoming? incoming;
    bool skipIncoming;
    Exception? callbackError;
    long receivedPublications,droppedPublications,droppedConnectionEvents;

    LwipMqttClient(LwipStack stack,LwipMqttOptions options)
    {
        this.stack=stack;this.options=options;
        publications=Channel.CreateBounded<LwipMqttPublication>(QueueOptions(options.PublicationQueueCapacity));
        connections=Channel.CreateBounded<LwipMqttConnectionEvent>(QueueOptions(options.ConnectionEventQueueCapacity));
    }
    static BoundedChannelOptions QueueOptions(int capacity)=>new(capacity)
    {SingleWriter=true,SingleReader=false,AllowSynchronousContinuations=false,FullMode=BoundedChannelFullMode.Wait};
    static LwipMqttOptions Validate(LwipMqttOptions value)
    {
        if(value.MaximumPendingRequests<1||value.MaximumPendingRequests>1024)throw new ArgumentOutOfRangeException(nameof(value.MaximumPendingRequests));
        if(value.PublicationQueueCapacity<1||value.PublicationQueueCapacity>4096)throw new ArgumentOutOfRangeException(nameof(value.PublicationQueueCapacity));
        if(value.ConnectionEventQueueCapacity<1||value.ConnectionEventQueueCapacity>4096)throw new ArgumentOutOfRangeException(nameof(value.ConnectionEventQueueCapacity));
        if(value.MaximumPublicationBytes<0||value.MaximumPublicationBytes>16*1024*1024)throw new ArgumentOutOfRangeException(nameof(value.MaximumPublicationBytes));
        if(value.MaximumTopicBytes<1||value.MaximumTopicBytes>65533)throw new ArgumentOutOfRangeException(nameof(value.MaximumTopicBytes));
        if(value.MaximumOutgoingPayloadBytes<0||value.MaximumOutgoingPayloadBytes>65535)throw new ArgumentOutOfRangeException(nameof(value.MaximumOutgoingPayloadBytes));
        // Queue + one assembling publication; strings need at most two bytes per
        // UTF-8 input byte. Object overhead and consumer-owned reads are separate.
        long bound=(long)(value.PublicationQueueCapacity+1)*(value.MaximumPublicationBytes+2L*value.MaximumTopicBytes);
        if(bound>128L*1024*1024)throw new ArgumentException("Configured MQTT queue payload/topic budget exceeds 128 MiB.",nameof(value));
        return value with{};
    }
    /// <summary>Create the owner before connecting. Original profile limits can be lower than the managed admission limits.</summary>
    public static LwipMqttClient Create(LwipStack stack,LwipMqttOptions? options=null)
    {
        ArgumentNullException.ThrowIfNull(stack);var selected=Validate(options??new());
        return stack.StartInitializedService(s=>new LwipMqttClient(s,selected),service=>service.Initialize());
    }
    void Initialize()=>stack.RunServiceBoundary(()=>stack.Invoke(owner=>
    {
        registry=Registries.GetValue(owner,static _=>new());registry.Clients.EnsureCapacity(registry.Clients.Count+1);
        root=GCHandle.Alloc(this);token=GCHandle.ToIntPtr(root);registry.Clients.Add(token,this);registered=true;
        client=(nint)owner.mqtt_client_new();if(client==0)throw new OutOfMemoryException("Original MQTT client allocation failed.");
        owner.mqtt_set_inpub_callback((mqtt_client_s*)client,&Publication,&Data,(void*)token);
    }));
    void Boundary(Action<Lwip> action)=>stack.RunServiceBoundary(()=>
    {ObjectDisposedException.ThrowIf(closing||disposed,this);stack.Invoke(action);});
    static byte[] Text(string value,int maximum,bool empty=true)
    {
        ArgumentNullException.ThrowIfNull(value);
        if(value.Contains('\0')||(!empty&&value.Length==0))throw new ArgumentException("MQTT string must satisfy the original non-NUL string contract.",nameof(value));
        int count=Utf8.GetByteCount(value);if(count>maximum)throw new ArgumentOutOfRangeException(nameof(value));
        byte[] bytes=new byte[count+1];Utf8.GetBytes(value,bytes);return bytes;
    }
    static byte[]? OptionalText(string? value,int maximum,bool empty=true)=>value==null?null:Text(value,maximum,empty);
    static void Quality(byte value){if(value>2)throw new ArgumentOutOfRangeException(nameof(value),"Original MQTT supports QoS 0, 1 and 2.");}

    /// <summary>Connect/reconnect using original MQTT clean-session behavior. Ordinary CONNACK refusals return their status. Synchronous original submission errors fault the Task.</summary>
    public Task<LwipMqttConnectionStatus> ConnectAsync(LwipEndpoint endpoint,LwipMqttConnectOptions configuration)
    {
        ArgumentNullException.ThrowIfNull(endpoint);ArgumentNullException.ThrowIfNull(configuration);Quality(configuration.WillQualityOfService);
        Task<LwipMqttConnectionStatus>? result=null;
        Boundary(owner=>
        {
            if(connected||connecting!=null||pending.Count!=0)throw new InvalidOperationException("This MQTT client is connected, connecting, or still retains requests needing cleanup.");
            if(configuration.WillPayload.Length>255)throw new ArgumentOutOfRangeException(nameof(configuration.WillPayload),"Original CONNECT Will length is limited to one byte.");
            byte[] id=Text(configuration.ClientId,65535),willPayload=configuration.WillPayload.ToArray();
            byte[]? user=OptionalText(configuration.UserName,65535,false),password=OptionalText(configuration.Password,65535),willTopic=OptionalText(configuration.WillTopic,255,false);
            if(willTopic==null&&willPayload.Length!=0)throw new ArgumentException("A Will payload requires its topic.",nameof(configuration));
            byte[]? terminatedWill=null;if(willTopic!=null){terminatedWill=new byte[willPayload.Length+1];willPayload.CopyTo(terminatedWill,0);}
            var completion=new TaskCompletionSource<LwipMqttConnectionStatus>(TaskCreationOptions.RunContinuationsAsynchronously);connecting=completion;result=completion.Task;
            try
            {
                ip_addr address;endpoint.WriteIp(&address);
                fixed(byte* clientId=id,username=user,pass=password,topic=willTopic,payload=terminatedWill)
                {
                    mqtt_connect_client_info_t info=default;info.client_id=clientId;info.client_user=username;info.client_pass=pass;info.keep_alive=configuration.KeepAliveSeconds;
                    info.will_topic=topic;info.will_msg=payload;info.will_msg_len=(byte)willPayload.Length;info.will_qos=configuration.WillQualityOfService;info.will_retain=(byte)(configuration.WillRetain?1:0);
                    int error=owner.mqtt_client_connect((mqtt_client_s*)client,&address,endpoint.Port,&Connection,(void*)token,&info);
                    if(error!=0){connecting=null;completion.TrySetException(new LwipMqttException("connect submission",error));}
                }
            }
            catch(Exception failure)
            {
                // A managed failure unwinds the original call before lifecycle
                // cleanup. Keep this owner/root alive through original stop.
                owner.mqtt_disconnect((mqtt_client_s*)client);connected=false;connecting=null;completion.TrySetException(failure);
                FinishAll(failure,false);ResetIncoming();
            }
        });
        return result!;
    }
    public bool IsConnected{get{bool value=false;Boundary(_=>value=connected);return value;}}
    public int PendingRequests{get{int count=0;Boundary(_=>count=pending.Count);return count;}}
    public long ReceivedPublications=>Interlocked.Read(ref receivedPublications);
    public long DroppedPublications=>Interlocked.Read(ref droppedPublications);
    public long DroppedConnectionEvents=>Interlocked.Read(ref droppedConnectionEvents);
    public bool TryReadPublication(out LwipMqttPublication? publication)=>publications.Reader.TryRead(out publication);
    public ValueTask<LwipMqttPublication> ReadPublicationAsync(CancellationToken cancellation=default)=>publications.Reader.ReadAsync(cancellation);
    public bool TryReadConnectionEvent(out LwipMqttConnectionEvent value)=>connections.Reader.TryRead(out value);
    public ValueTask<LwipMqttConnectionEvent> ReadConnectionEventAsync(CancellationToken cancellation=default)=>connections.Reader.ReadAsync(cancellation);

    /// <summary>Completes with the original request callback: QoS 0 is TCP-sent notification; QoS 1/2 follow the original acknowledgement path. The wrapper adds no delivery guarantee.</summary>
    public Task PublishAsync(string topic,ReadOnlyMemory<byte> payload,byte qualityOfService=0,bool retain=false)
    {
        Quality(qualityOfService);
        return Submit("publish",owner=>
        {
            if(payload.Length>options.MaximumOutgoingPayloadBytes)throw new ArgumentOutOfRangeException(nameof(payload));
            byte[] name=Text(topic,options.MaximumTopicBytes,false),copy=payload.ToArray();
            return argument=>{fixed(byte* text=name,bytes=copy)return owner.mqtt_publish((mqtt_client_s*)client,text,bytes,(ushort)copy.Length,qualityOfService,(byte)(retain?1:0),&RequestCompleted,(void*)argument);};
        });
    }
    public Task SubscribeAsync(string topic,byte maximumQualityOfService=0)
    {
        Quality(maximumQualityOfService);return Submit("subscribe",owner=>
        {byte[] name=Text(topic,options.MaximumTopicBytes,false);return argument=>{fixed(byte* text=name)return owner.mqtt_sub_unsub((mqtt_client_s*)client,text,maximumQualityOfService,&RequestCompleted,(void*)argument,1);};});
    }
    public Task UnsubscribeAsync(string topic)=>Submit("unsubscribe",owner=>
    {byte[] name=Text(topic,options.MaximumTopicBytes,false);return argument=>{fixed(byte* text=name)return owner.mqtt_sub_unsub((mqtt_client_s*)client,text,0,&RequestCompleted,(void*)argument,0);};});
    Task Submit(string operation,Func<Lwip,Func<nint,int>> prepare)
    {
        Task? task=null;Boundary(owner=>
        {
            if(!connected)throw new InvalidOperationException("Original MQTT connection is not accepted.");
            if(pending.Count>=options.MaximumPendingRequests)throw new InvalidOperationException("MQTT pending-request admission limit reached.");
            // Validate/copy before adopting a request. Ordinary API argument
            // errors must not disconnect other admitted requests.
            var submit=prepare(owner);
            var request=new Request(this,operation);task=request.Completion.Task;
            registry!.Requests.EnsureCapacity(registry.Requests.Count+1);pending.EnsureCapacity(pending.Count+1);
            request.Root=GCHandle.Alloc(request);nint argument=GCHandle.ToIntPtr(request.Root);
            try{registry.Requests.Add(argument,request);pending.Add(argument,request);}
            catch{registry.Requests.Remove(argument);pending.Remove(argument);request.Root.Free();throw;}
            try
            {
                int error=submit(argument);
                if(error!=0)FinishRequest(argument,request,new LwipMqttException(operation,error),false);
            }
            catch(Exception failure)
            {
                // No public per-request cancel exists. Stop the original client
                // before releasing any request root it might have admitted.
                owner.mqtt_disconnect((mqtt_client_s*)client);connected=false;ResetIncoming();FinishAll(failure,false);
                ConnectionEvent(LwipMqttConnectionStatus.Disconnected,true);
            }
        });return task!;
    }
    void FinishRequest(nint argument,Request request,Exception? error,bool canceled)
    {
        // Original callbacks no longer use callback_arg after returning; the C
        // request record itself remains original-owned and is deleted by lwIP.
        if(request.Root.IsAllocated)request.Root.Free();registry!.Requests.Remove(argument);pending.Remove(argument);
        if(error!=null)request.Completion.TrySetException(error);
        else if(canceled)request.Completion.TrySetCanceled();
        else request.Completion.TrySetResult();
    }
    void FinishAll(Exception? error,bool canceled)
    {
        // Snapshot allocation is bounded by admission, before any root release.
        var snapshot=new KeyValuePair<nint,Request>[pending.Count];int index=0;foreach(var entry in pending)snapshot[index++]=entry;
        foreach(var entry in snapshot)FinishRequest(entry.Key,entry.Value,error,canceled);
    }
    static bool Find(Lwip owner,void* argument,out LwipMqttClient service)
    {service=null!;return Registries.TryGetValue(owner,out var registry)&&registry.Clients.TryGetValue((nint)argument,out service!);}
    static void Connection(Lwip owner,mqtt_client_s* native,void* argument,mqtt_connection_status_t status)
    {
        if(!Find(owner,argument,out var service))return;
        try
        {
            if((nint)native!=service.client)throw new InvalidOperationException("MQTT callback client/owner identity mismatch.");
            var result=(LwipMqttConnectionStatus)status;service.connected=result==LwipMqttConnectionStatus.Accepted;
            var completion=service.connecting;service.connecting=null;completion?.TrySetResult(result);
            if(!service.connected){service.ResetIncoming();service.FinishAll(new LwipMqttDisconnectedException(result),false);}
            service.ConnectionEvent(result,false);
        }
        catch(Exception failure){service.Remember(failure);}
    }
    static void RequestCompleted(Lwip owner,void* argument,sbyte error)
    {
        if(!Registries.TryGetValue(owner,out var registry)||!registry.Requests.TryGetValue((nint)argument,out var request))return;
        try{request.Service.FinishRequest((nint)argument,request,error==0?null:new LwipMqttException(request.Operation,error),false);}
        catch(Exception failure){request.Service.Remember(failure);request.Completion.TrySetException(failure);}
    }
    void ConnectionEvent(LwipMqttConnectionStatus status,bool local)
    {if(!connections.Writer.TryWrite(new(status,local)))Interlocked.Increment(ref droppedConnectionEvents);}
    void Remember(Exception failure){callbackError??=failure;}
    void ResetIncoming(){incoming=null;skipIncoming=false;}
    void Deliver(string topic,byte[] payload)
    {
        Interlocked.Increment(ref receivedPublications);
        if(!publications.Writer.TryWrite(new(topic,payload)))Interlocked.Increment(ref droppedPublications);
    }
    static void Publication(Lwip owner,void* argument,byte* topic,uint length)
    {
        if(!Find(owner,argument,out var service))return;
        try
        {
            service.ResetIncoming();
            int topicLength=0;while(topicLength<=service.options.MaximumTopicBytes&&topic[topicLength]!=0)topicLength++;
            if(topicLength>service.options.MaximumTopicBytes||length>service.options.MaximumPublicationBytes)
            {service.skipIncoming=true;Interlocked.Increment(ref service.droppedPublications);return;}
            string copied=Utf8.GetString(new ReadOnlySpan<byte>(topic,topicLength));
            if(length==0)
            {
                service.Deliver(copied,Array.Empty<byte>());
                // Header documentation permits no data callback for empty data;
                // the pinned implementation also emits a zero-length LAST call.
                service.skipIncoming=true;return;
            }
            service.incoming=new(copied,(int)length);
        }
        catch(Exception failure){service.Remember(failure);service.incoming=null;service.skipIncoming=true;Interlocked.Increment(ref service.droppedPublications);}
    }
    static void Data(Lwip owner,void* argument,byte* bytes,ushort length,byte flags)
    {
        if(!Find(owner,argument,out var service))return;
        try
        {
            if(service.skipIncoming){if((flags&1)!=0)service.skipIncoming=false;return;}
            var value=service.incoming??throw new InvalidOperationException("Original MQTT data callback lacks a publication header.");
            if(length>value.Bytes.Length-value.Position)throw new InvalidOperationException("Original MQTT notification exceeds its announced length.");
            new ReadOnlySpan<byte>(bytes,length).CopyTo(value.Bytes.AsSpan(value.Position));value.Position+=length;
            if((flags&1)!=0)
            {
                if(value.Position!=value.Bytes.Length)throw new InvalidOperationException("Original MQTT LAST callback precedes its announced length.");
                service.incoming=null;service.Deliver(value.Topic,value.Bytes);
            }
        }
        catch(Exception failure){service.Remember(failure);service.incoming=null;service.skipIncoming=(flags&1)==0;Interlocked.Increment(ref service.droppedPublications);}
    }
    void StopOriginal(Lwip owner)
    {
        bool active=connected||connecting!=null;
        if(client!=0)owner.mqtt_disconnect((mqtt_client_s*)client);
        connected=false;var attempt=connecting;connecting=null;attempt?.TrySetCanceled();FinishAll(null,true);ResetIncoming();
        if(active)ConnectionEvent(LwipMqttConnectionStatus.Disconnected,true);
    }
    /// <summary>Stop the whole original connection locally and cancel its pending requests. Upstream supplies neither individual request cancellation nor a graceful DISCONNECT-packet entry.</summary>
    public void Disconnect()=>stack.RunServiceBoundary(()=>
    {
        ObjectDisposedException.ThrowIf(closing||disposed,this);stack.Invoke(StopOriginal);stack.ServiceBarrier();
    });
    public void CancelPending()=>Disconnect();
    public void ThrowCallbackErrors()=>Boundary(_=>
    {if(callbackError is{} failure){callbackError=null;throw new AggregateException("MQTT callback delivery failed.",failure);}});
    public void Dispose()
    {
        if(disposed)return;
        try{stack.RunServiceBoundary(()=>
        {
            if(disposed)return;closing=true;
            if(!stopped){stack.Invoke(StopOriginal);stopped=true;}
            stack.ServiceBarrier();
            stack.Invoke(owner=>
            {
                if(pending.Count!=0)throw new InvalidOperationException("MQTT request roots remain live; retain owner for cleanup retry.");
                if(client!=0){owner.mqtt_set_inpub_callback((mqtt_client_s*)client,null,null,null);owner.mqtt_client_free((mqtt_client_s*)client);client=0;}
                if(registered){registry!.Clients.Remove(token);registered=false;}
                if(root.IsAllocated)root.Free();token=0;
            });
            publications.Writer.TryComplete(new ObjectDisposedException(nameof(LwipMqttClient)));
            connections.Writer.TryComplete(new ObjectDisposedException(nameof(LwipMqttClient)));
            disposed=true;stack.CompleteServiceDisposal(this);
        });}catch(ObjectDisposedException)when(disposed){}
    }
}
