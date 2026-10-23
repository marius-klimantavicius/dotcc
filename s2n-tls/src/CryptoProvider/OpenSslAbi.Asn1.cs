using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Asn1;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    // These two layouts are independently measured against the selected headers
    // and the actual emitted structs, not inferred from managed object layout.
    [StructLayout(LayoutKind.Sequential)]
    private struct AsnStringNative {public int Length,Type;public byte* Data;public long Flags;}
    [StructLayout(LayoutKind.Sequential)]
    private struct GeneralNameNative {public int Type;public void* Value;}
    private static void* AddAt(void* pointer,object value)
    {
        if(pointer==null)throw new OutOfMemoryException();if(!Handles.TryAdd((nint)pointer,value))throw new InvalidOperationException();Interlocked.Increment(ref transientHandles);return pointer;
    }
    private sealed class AsnStringState : IDisposable
    {
        public readonly nint Pointer;
        public string? Text;
        public DateTime? Time;
        public AsnStringState(int type,ReadOnlySpan<byte> data,string? text=null,DateTime? time=null)
        {
            var value=(AsnStringNative*)NativeMemory.AllocZeroed((nuint)sizeof(AsnStringNative));
            if(value==null)throw new OutOfMemoryException();
            try{value->Type=type;value->Length=data.Length;value->Data=(byte*)NativeMemory.Alloc((nuint)Math.Max(1,data.Length));if(value->Data==null)throw new OutOfMemoryException();data.CopyTo(Output(value->Data,data.Length));AddAt(value,this);Pointer=(nint)value;Text=text;Time=time;}
            catch{NativeMemory.Free(value->Data);NativeMemory.Free(value);throw;}
        }
        public void Dispose(){var value=(AsnStringNative*)Pointer;NativeMemory.Free(value->Data);value->Data=null;}
    }
    private static nint Asn(int type,ReadOnlySpan<byte> bytes,string? text=null,DateTime? time=null)=>new AsnStringState(type,bytes,text,time).Pointer;
    private static nint AsnObject(Asn1Encodable value)
    {
        byte[] encoded=value.GetDerEncoded();
        var tag=System.Formats.Asn1.AsnDecoder.ReadEncodedValue(encoded,System.Formats.Asn1.AsnEncodingRules.DER,out int offset,out int count,out _);
        var raw=encoded.AsSpan(offset,count);
        string? text=tag.TagValue switch
        {
            12=>new UTF8Encoding(false,true).GetString(raw),
            30=>new UnicodeEncoding(true,false,true).GetString(raw),
            28=>new UTF32Encoding(true,false,true).GetString(raw),
            _=>value is IAsn1String str?str.GetString():null
        };
        return Asn(tag.TagValue,encoded.AsSpan(offset,count),text);
    }
    public static byte* ASN1_STRING_get0_data(void* value)
    {try{_=Get<AsnStringState>(value);return ((AsnStringNative*)value)->Data;}catch{Error();return null;}}
    public static int ASN1_STRING_length(void* value)
    {try{_=Get<AsnStringState>(value);return ((AsnStringNative*)value)->Length;}catch{Error();return 0;}}
    public static int ASN1_STRING_type(void* value)
    {try{_=Get<AsnStringState>(value);return ((AsnStringNative*)value)->Type;}catch{Error();return 0;}}
    public static void ASN1_STRING_free(void* value){try{Remove(value);}catch{Error();}}
    public static int ASN1_STRING_to_UTF8(byte** output,void* value)
    {
        try
        {
            if(output==null)throw new ArgumentException();var state=Get<AsnStringState>(value);string text=state.Text??new UTF8Encoding(false,true).GetString(Input(((AsnStringNative*)value)->Data,(ulong)((AsnStringNative*)value)->Length));
            byte[] bytes=Encoding.UTF8.GetBytes(text);*output=(byte*)NativeMemory.Alloc((nuint)(bytes.Length+1));if(*output==null)throw new OutOfMemoryException();bytes.CopyTo(Output(*output,bytes.Length));(*output)[bytes.Length]=0;return bytes.Length;
        }
        catch{if(output!=null)*output=null;Error();return -1;}
    }
    public static void* d2i_ASN1_UTF8STRING(void** output,byte** input,long length)
    {
        nint handle=0;
        try
        {
            if(input==null||length<0)throw new ArgumentException();var reader=new System.Formats.Asn1.AsnReader(Input(*input,(ulong)length).ToArray(),System.Formats.Asn1.AsnEncodingRules.DER);var encoded=reader.ReadEncodedValue();var text=DerUtf8String.GetInstance(encoded.ToArray());handle=AsnObject(text);
            if(output!=null){if(*output!=null)Remove(*output);*output=(void*)handle;}*input+=encoded.Length;return (void*)handle;
        }
        catch{if(handle!=0)Remove((void*)handle);Error();return null;}
    }
    public static void* ASN1_GENERALIZEDTIME_set(void* value,long seconds)
    {
        try
        {
            DateTime time=DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;string text=time.ToString("yyyyMMddHHmmss'Z'",CultureInfo.InvariantCulture);byte[] bytes=Encoding.ASCII.GetBytes(text);
            if(value==null)return (void*)Asn(24,bytes,text,time);
            var state=Get<AsnStringState>(value);var native=(AsnStringNative*)value;byte* data=(byte*)NativeMemory.Alloc((nuint)bytes.Length);if(data==null)throw new OutOfMemoryException();bytes.CopyTo(Output(data,bytes.Length));NativeMemory.Free(native->Data);native->Data=data;native->Length=bytes.Length;native->Type=24;state.Text=text;state.Time=time;return value;
        }
        catch{Error();return null;}
    }
    private static DateTime AsnTime(void* value)=>value==null?DateTime.UtcNow:Get<AsnStringState>(value).Time??throw new ArgumentException("Expected ASN1_TIME.");
    public static int ASN1_TIME_diff(int* days,int* seconds,void* from,void* to)
    {
        try{long delta=(AsnTime(to).Ticks-AsnTime(from).Ticks)/TimeSpan.TicksPerSecond;if(days!=null)*days=checked((int)(delta/86400));if(seconds!=null)*seconds=checked((int)(delta%86400));return 1;}catch{Error();return 0;}
    }
    public static int X509_cmp_time(void* value,long* seconds)
    {try{var date=seconds==null?DateTime.UtcNow:DateTimeOffset.FromUnixTimeSeconds(*seconds).UtcDateTime;return AsnTime(value)<=date?-1:1;}catch{Error();return 0;}}
    private sealed record OidState(string Oid);
    private sealed record NidInfo(int Nid,string Oid,int Digest,int Key,string ShortName,string LongName);
    private static readonly Dictionary<int,NidInfo> Nids=LoadNids();
    private static readonly Dictionary<string,NidInfo> Oids=Nids.Values.GroupBy(x=>x.Oid).ToDictionary(x=>x.Key,x=>x.First());
    private static Dictionary<int,NidInfo> LoadNids()
    {
        using var stream=typeof(OpenSslAbi).Assembly.GetManifestResourceStream("Managed.S2nTls.Crypto.openssl-oid-map.json")??throw new InvalidOperationException("Missing pinned OID inventory.");
        using var document=JsonDocument.Parse(stream);var result=new Dictionary<int,NidInfo>();
        foreach(var entry in document.RootElement.GetProperty("nids").EnumerateArray())
        {int nid=entry.GetProperty("nid").GetInt32();result.Add(nid,new NidInfo(nid,entry.GetProperty("oid").GetString()!,entry.GetProperty("digest").GetInt32(),entry.GetProperty("key").GetInt32(),entry.GetProperty("short_name").GetString()!,entry.GetProperty("long_name").GetString()!));}
        return result;
    }
    private static int Nid(string oid)=>Oids.TryGetValue(oid,out var info)?info.Nid:0;
    public static int OBJ_obj2nid(void* value){try{return Nid(Get<OidState>(value).Oid);}catch{Error();return 0;}}
    public static int OBJ_cmp(void* first,void* second){try{return new DerObjectIdentifier(Get<OidState>(first).Oid).GetDerEncoded().AsSpan().SequenceCompareTo(new DerObjectIdentifier(Get<OidState>(second).Oid).GetDerEncoded());}catch{Error();return -1;}}
    public static void* OBJ_txt2obj(byte* input,int noName)
    {
        try
        {
            string value=Marshal.PtrToStringUTF8((nint)input)??throw new ArgumentException();if(noName==0){var found=Nids.Values.FirstOrDefault(x=>x.ShortName==value||x.LongName==value);if(found!=null)value=found.Oid;}
            _=new DerObjectIdentifier(value);return Add(new OidState(value));
        }
        catch{Error();return null;}
    }
    public static void ASN1_OBJECT_free(void* value){try{Remove(value);}catch{Error();}}
    public static int OBJ_find_sigid_algs(int nid,int* digest,int* key)
    {try{if(!Nids.TryGetValue(nid,out var info)||info.Key==0)return 0;if(digest!=null)*digest=info.Digest;if(key!=null)*key=info.Key;return 1;}catch{Error();return 0;}}
}
