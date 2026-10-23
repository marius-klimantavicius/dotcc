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
        byte* allocated=null;
        try
        {
            if(value==null)return -1;
            _=Get<AsnStringState>(value);
            var native=(AsnStringNative*)value;
            int width=native->Type switch
            {
                12=>0,
                18 or 19 or 20 or 22 or 23 or 24 or 26=>1,
                28=>4,
                30=>2,
                _=>-1
            };
            // Native rejects unsupported tags before mbstring conversion,
            // without an error entry or a write to the caller's output pointer.
            if(width<0)return -1;
            if(output==null)throw new ArgumentException();
            int length=native->Length;
            if(length==-1)
            {
                if(native->Data==null)throw new ArgumentException();
                length=0;while(native->Data[length]!=0)length=checked(length+1);
            }
            var input=Input(native->Data,checked((ulong)length));
            byte[] bytes;
            if(width==0)
            {
                Asn1ValidateNativeUtf8(input);
                bytes=input.ToArray();
            }
            else if(width==1)bytes=Encoding.UTF8.GetBytes(Encoding.Latin1.GetString(input));
            else
            {
                if(length%width!=0)throw new FormatException("Invalid native ASN.1 string width.");
                var encoded=new List<byte>(length);
                for(int i=0;i<input.Length;i+=width)
                {
                    uint code=width==2?global::System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(input.Slice(i,2))
                        :global::System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(input.Slice(i,4));
                    Asn1EncodeNativeUtf8(encoded,code);
                }
                bytes=encoded.ToArray();
            }
            allocated=(byte*)NativeMemory.Alloc(checked((nuint)bytes.Length+1));
            if(allocated==null)throw new OutOfMemoryException();
            bytes.CopyTo(Output(allocated,bytes.Length));allocated[bytes.Length]=0;
            *output=allocated;allocated=null;return bytes.Length;
        }
        catch{NativeMemory.Free(allocated);Error();return -1;}
    }
    private static void Asn1ValidateNativeUtf8(ReadOnlySpan<byte> input)
    {
        for(int i=0;i<input.Length;)
        {
            byte first=input[i];
            if(first<0x80){i++;continue;}
            int width=(first&0xe0)==0xc0?2:(first&0xf0)==0xe0?3:(first&0xf8)==0xf0?4
                :(first&0xfc)==0xf8?5:(first&0xfe)==0xfc?6:0;
            if(width==0||input.Length-i<width)throw new FormatException("Invalid native UTF8 string.");
            uint code=(uint)(first&((1<<(7-width))-1));
            for(int j=1;j<width;j++)
            {
                byte next=input[i+j];if((next&0xc0)!=0x80)throw new FormatException("Invalid native UTF8 continuation.");
                code=(code<<6)|(uint)(next&0x3f);
            }
            uint minimum=width switch{2=>0x80,3=>0x800,4=>0x10000,5=>0x200000,_=>0x4000000};
            if(code<minimum)throw new FormatException("Nonminimal native UTF8 string.");
            i+=width;
        }
    }
    private static void Asn1EncodeNativeUtf8(List<byte> output,uint code)
    {
        // The selected native declaration backend predates Unicode scalar
        // restrictions here: BMP surrogates and 5/6-byte forms are preserved.
        // This boundary conversion is not a general .NET text decoder.
        if(code<0x80){output.Add((byte)code);return;}
        int width=code<0x800?2:code<0x10000?3:code<0x200000?4:code<0x4000000?5:6;
        int prefix=width switch{2=>0xc0,3=>0xe0,4=>0xf0,5=>0xf8,_=>0xfc};
        output.Add((byte)((uint)prefix|((code>>(6*(width-1)))&(uint)((1<<(7-width))-1))));
        for(int j=width-2;j>=0;j--)output.Add((byte)(0x80|((code>>(6*j))&0x3f)));
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
