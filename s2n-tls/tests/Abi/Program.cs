using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using static Managed.Security.S2n.S2nTls;
using Managed.S2nTls.Crypto;

unsafe
{
    if(args.Length!=3)throw new ArgumentException("Usage: ProviderAbiProbe <native-probe-output> <report.json> <certificate.pem>");
    var lines=File.ReadAllLines(args[0]);var fields=new ulong[17];
    for(int i=0;i<fields.Length;i++)
    {
        fields[i]=provider_layout_value(i);
        if(fields[i]!=ulong.Parse(lines[i]))throw new Exception($"Layout field {i} mismatch: {fields[i]} != {lines[i]}");
    }
    int before=OpenSslAbi.LiveContexts;
    ulong capabilities=provider_runtime_contract_probe();
    if(capabilities!=0xff||capabilities!=OpenSslAbi.dotcc_crypto_provider_capabilities())throw new Exception("Actual emitted provider ABI/name/initialization/capability handshake failed.");
    byte[] data=[0,1,128,255],digest=new byte[36];
    fixed(byte* input=data)fixed(byte* output=digest)
    {
        if(provider_digest_probe(input,(ulong)data.Length,output)!=1)throw new Exception("Actual emitted EVP digest probe failed.");
        if(Convert.ToHexString(digest).ToLowerInvariant()!=lines[17])throw new Exception("Emitted digest differs from native.");
    }
    string pem=File.ReadAllText(args[2]);var field=PemEncoding.Find(pem);byte[] der=Convert.FromBase64String(pem[field.Base64Data]);byte[] name=new byte[512];
    fixed(byte* encoded=der)fixed(byte* output=name)
    {int count=provider_certificate_san_probe(encoded,(ulong)der.Length,output,name.Length);if(count!=16||!name.AsSpan(0,count).SequenceEqual("provider.example"u8))throw new Exception("Actual emitted GENERAL_NAME/ASN1_STRING field access failed.");}
    int* context=(int*)NativeMemory.Alloc((nuint)sizeof(int));*context=7;
    try
    {
        delegate*<void*,byte*,ulong,int> callback=&Callback.Entry;
        if(provider_register_callback(callback,context)!=1||provider_callback_identity(callback)!=1)throw new Exception("Callback registration/identity failed.");
        GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
        fixed(byte* input=data)if(provider_invoke_callback(input,(ulong)data.Length)!=391||*context!=391)throw new Exception("Retained emitted callback failed after GC.");
        provider_clear_callback();fixed(byte* input=data)if(provider_invoke_callback(input,(ulong)data.Length)!=-1)throw new Exception("Callback did not clear.");
    }
    finally{provider_clear_callback();NativeMemory.Free(context);}
    if(OpenSslAbi.LiveContexts!=before)throw new Exception("Emitted probe leaked provider contexts.");
    using var report=File.Create(args[1]);using var writer=new Utf8JsonWriter(report,new JsonWriterOptions{Indented=true});
    writer.WriteStartObject();writer.WriteString("scope","actual emitted pinned-header layouts/GENERAL_NAME/ASN1_STRING field access, EVP digest copy/final/free, retained callback/identity/GC; not upstream s2n wrapper/TLS qualification");
    writer.WriteString("status","passed");writer.WriteNumber("provider_capabilities",capabilities);writer.WriteStartArray("layout_values");foreach(var value in fields)writer.WriteNumberValue(value);writer.WriteEndArray();writer.WriteString("digest",Convert.ToHexString(digest));writer.WriteNumber("provider_live_context_delta",OpenSslAbi.LiveContexts-before);writer.WriteEndObject();
    Console.WriteLine("PASS emitted/native layouts, EVP digest, callback identity/lifetime/GC");
}

static unsafe class Callback
{
    public static int Entry(void* context,byte* bytes,ulong length)
    {
        try{if(context==null||length>4096)return -1;for(ulong i=0;i<length;i++)*(int*)context+=bytes[i];return *(int*)context;}
        catch{return -1;}
    }
}
