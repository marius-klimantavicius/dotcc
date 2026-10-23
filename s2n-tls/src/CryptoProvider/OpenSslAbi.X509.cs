using System.Buffers.Binary;
using System.Formats.Asn1;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private sealed class CertState(Certificate value) : IDisposable
    {
        public readonly Certificate Certificate=value;
        public readonly List<nint> Borrowed=new();
        public readonly Dictionary<string,nint> Cache=new();
        private int references=1;
        public CertState Retain(){Interlocked.Increment(ref references);return this;}
        public nint Borrow(string name,Func<nint> create)
        {lock(Cache){if(Cache.TryGetValue(name,out nint value))return value;value=create();Borrowed.Add(value);Cache.Add(name,value);return value;}}
        public void Dispose(){if(Interlocked.Decrement(ref references)!=0)return;foreach(nint h in Borrowed)Remove((void*)h);Borrowed.Clear();Cache.Clear();}
    }
    private sealed class NameState : IDisposable
    {
        public readonly byte[] Der,Canonical;
        public readonly List<(string Oid,nint Value)> Entries=new();
        public readonly nint DerPointer;
        public NameState(X509Name name)
        {
            Der=name.GetDerEncoded();using var canonical=new MemoryStream();
            foreach(Asn1Set rdn in Asn1Sequence.GetInstance(name.ToAsn1Object()))
            {
                var values=new List<Asn1Encodable>();
                foreach(Asn1Sequence attribute in rdn)
                {
                    string oid=DerObjectIdentifier.GetInstance(attribute[0]).Id;Entries.Add((oid,AsnObject(attribute[1])));
                    Asn1Encodable value=attribute[1];
                    var asn=Get<AsnStringState>((void*)Entries[^1].Value);
                    if(asn.Text is string text&&((AsnStringNative*)asn.Pointer)->Type is 12 or 19 or 20 or 22 or 26 or 28 or 30)
                    {
                        byte[] utf8=Encoding.UTF8.GetBytes(text);var cleaned=new List<byte>();int start=0,end=utf8.Length;
                        bool Space(byte b)=>b is 0x09 or 0x0a or 0x0b or 0x0c or 0x0d or 0x20;
                        while(start<end&&Space(utf8[start]))start++;while(end>start&&Space(utf8[end-1]))end--;
                        for(int i=start;i<end;i++){byte b=utf8[i];if(Space(b)){cleaned.Add(0x20);while(i+1<end&&Space(utf8[i+1]))i++;}else cleaned.Add(b is >=65 and <=90?(byte)(b+32):b);}
                        value=new DerUtf8String(new UTF8Encoding(false,true).GetString(cleaned.ToArray()));
                    }
                    values.Add(new DerSequence(attribute[0],value));
                }
                byte[] bytes=new DerSet(values.ToArray()).GetDerEncoded();canonical.Write(bytes);
            }
            Canonical=canonical.ToArray();DerPointer=(nint)NativeMemory.Alloc((nuint)Math.Max(1,Der.Length));if(DerPointer==0)throw new OutOfMemoryException();Der.CopyTo(Output((void*)DerPointer,Der.Length));
        }
        public void Dispose(){foreach(var entry in Entries)Remove((void*)entry.Value);NativeMemory.Free((void*)DerPointer);}
    }
    private sealed class ExtensionState(string oid,Org.BouncyCastle.Asn1.X509.X509Extension extension) : IDisposable
    {
        public readonly string Oid=oid;
        public readonly bool Critical=extension.IsCritical;
        public readonly nint Object=(nint)Add(new OidState(oid));
        public readonly nint Data=Asn(4,extension.Value.GetOctets());
        public void Dispose(){Remove((void*)Object);Remove((void*)Data);}
    }
    private sealed class GeneralNameState : IDisposable
    {
        public readonly nint Pointer,Value;
        public GeneralNameState(int type,nint value)
        {
            var data=(GeneralNameNative*)NativeMemory.AllocZeroed((nuint)sizeof(GeneralNameNative));if(data==null)throw new OutOfMemoryException();data->Type=type;data->Value=(void*)value;Value=value;Pointer=(nint)AddAt(data,this);
        }
        public void Dispose()=>Remove((void*)Value);
    }
    public static void* d2i_X509(void** output,byte** input,long length)
    {
        CertState? state=null;
        try
        {
            if(input==null||length<0)throw new ArgumentException();var cert=Certificate.ParseDer(Input(*input,(ulong)length),out int consumed);state=new CertState(cert);void* handle=Add(state);state=null;
            if(output!=null){if(*output!=null)Remove(*output);*output=handle;}*input+=consumed;return handle;
        }
        catch{state?.Dispose();Error();return null;}
    }
    public static int i2d_X509(void* cert,byte** output)
    {
        try
        {
            byte[] der=Get<CertState>(cert).Certificate.Der;if(output!=null)
            {
                if(*output==null){*output=(byte*)NativeMemory.Alloc((nuint)der.Length);if(*output==null)throw new OutOfMemoryException();der.CopyTo(Output(*output,der.Length));}
                else{der.CopyTo(Output(*output,der.Length));*output+=der.Length;}
            }
            return der.Length;
        }
        catch{Error();return -1;}
    }
    public static void X509_free(void* cert){try{Remove(cert);}catch{Error();}}
    public static void* X509_get_pubkey(void* cert){try{return Add(ParsePublic(Get<CertState>(cert).Certificate.PublicKeyDer));}catch{Error();return null;}}
    public static int X509_get_signature_nid(void* cert){try{return Nid(Get<CertState>(cert).Certificate.SignatureAlgorithmOid);}catch{Error();return 0;}}
    public static void* X509_get_subject_name(void* cert)
    {try{var c=Get<CertState>(cert);return (void*)c.Borrow("subject",()=>(nint)Add(new NameState(c.Certificate.Value.SubjectDN)));}catch{Error();return null;}}
    public static void* X509_get_issuer_name(void* cert)
    {try{var c=Get<CertState>(cert);return (void*)c.Borrow("issuer",()=>(nint)Add(new NameState(c.Certificate.Value.IssuerDN)));}catch{Error();return null;}}
    public static int X509_NAME_cmp(void* first,void* second)
    {try{var a=Get<NameState>(first).Canonical;var b=Get<NameState>(second).Canonical;return a.Length!=b.Length?a.Length-b.Length:a.AsSpan().SequenceCompareTo(b);}catch{Error();return -1;}}
    public static ulong X509_NAME_hash(void* name)
    {try{byte[] hash=SHA1.HashData(Get<NameState>(name).Canonical);return BinaryPrimitives.ReadUInt32LittleEndian(hash);}catch{Error();return 0;}}
    public static ulong X509_issuer_name_hash(void* cert)=>X509_NAME_hash(X509_get_issuer_name(cert));
    public static int X509_NAME_get0_der(void* name,byte** output,ulong* length)
    {try{var n=Get<NameState>(name);if(output!=null)*output=(byte*)n.DerPointer;if(length!=null)*length=(ulong)n.Der.Length;return 1;}catch{Error();return 0;}}
    public static int X509_NAME_get_index_by_NID(void* name,int nid,int last)
    {try{var n=Get<NameState>(name);if(!Nids.TryGetValue(nid,out var info))return -1;for(int i=Math.Max(0,last+1);i<n.Entries.Count;i++)if(n.Entries[i].Oid==info.Oid)return i;return -1;}catch{Error();return -1;}}
    // Name-entry handles refer to the same real ASN1_STRING storage. Its owner is the name.
    public static void* X509_NAME_get_entry(void* name,int index)
    {try{var n=Get<NameState>(name);return index<0||index>=n.Entries.Count?null:(void*)n.Entries[index].Value;}catch{Error();return null;}}
    public static void* X509_NAME_ENTRY_get_data(void* entry){try{_=Get<AsnStringState>(entry);return entry;}catch{Error();return null;}}
    public static int X509_get_ext_count(void* cert)
    {try{return Get<CertState>(cert).Certificate.Value.CertificateStructure.TbsCertificate.Extensions?.GetExtensionOids().Length??0;}catch{Error();return -1;}}
    public static void* X509_get_ext(void* cert,int index)
    {
        try
        {
            var c=Get<CertState>(cert);var extensions=c.Certificate.Value.CertificateStructure.TbsCertificate.Extensions??throw new ArgumentException();var oids=extensions.GetExtensionOids();if(index<0||index>=oids.Length)return null;
            return (void*)c.Borrow("ext"+index,()=>(nint)Add(new ExtensionState(oids[index].Id,extensions.GetExtension(oids[index]))));
        }
        catch{Error();return null;}
    }
    public static int X509_EXTENSION_get_critical(void* extension){try{return Get<ExtensionState>(extension).Critical?1:0;}catch{Error();return 0;}}
    public static void* X509_EXTENSION_get_data(void* extension){try{return (void*)Get<ExtensionState>(extension).Data;}catch{Error();return null;}}
    public static void* X509_EXTENSION_get_object(void* extension){try{return (void*)Get<ExtensionState>(extension).Object;}catch{Error();return null;}}
    public static void* X509_get_ext_d2i(void* cert,int nid,int* critical,int* index)
    {
        nint stack=0;
        try
        {
            if(nid!=Nid("2.5.29.17"))throw new NotSupportedException("s2n decodes subjectAltName via this API.");
            var c=Get<CertState>(cert);var extension=c.Certificate.Value.GetExtensionValue(X509Extensions.SubjectAlternativeName);
            if(extension==null){if(critical!=null)*critical=-1;return null;}
            if(index!=null&&*index>=0)return null;if(index!=null)*index=0;
            if(critical!=null)*critical=c.Certificate.Value.GetCriticalExtensionOids()?.Contains("2.5.29.17")==true?1:0;
            stack=(nint)OPENSSL_sk_new_null();var values=Get<StackState>((void*)stack).Values;
            foreach(var name in GeneralNames.GetInstance(Asn1Object.FromByteArray(extension.GetOctets())).GetNames())
            {
                nint value=name.TagNo switch
                {
                    GeneralName.DnsName or GeneralName.UniformResourceIdentifier or GeneralName.Rfc822Name=>AsnObject(name.Name),
                    GeneralName.IPAddress=>Asn(4,Asn1OctetString.GetInstance(name.Name).GetOctets()),
                    _=>Asn(4,name.Name.GetDerEncoded())
                };
                try{values.Add(new GeneralNameState(name.TagNo,value).Pointer);}catch{Remove((void*)value);throw;}
            }
            return (void*)stack;
        }
        catch{if(stack!=0)GENERAL_NAMES_free((void*)stack);Error();return null;}
    }
    public static void GENERAL_NAMES_free(void* names)
    {try{if(names==null)return;foreach(nint value in Get<StackState>(names).Values)Remove((void*)value);Remove(names);}catch{Error();}}
    public static int X509_check_issued(void* issuer,void* subject)
    {
        try
        {
            var a=Get<CertState>(issuer).Certificate.Value;var b=Get<CertState>(subject).Certificate.Value;
            if(!a.SubjectDN.Equivalent(b.IssuerDN,true))return 29;
            int authority=CheckAuthorityIdentifier(a,AuthorityKeyIdentifier.GetInstance(ExtensionValue(b,X509Extensions.AuthorityKeyIdentifier)));if(authority!=0)return authority;
            if(!Oids.TryGetValue(b.SigAlgOid,out var signature)||signature.Key==0)return 76;
            int actual=a.GetPublicKey() switch{Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters=>6,Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters=>408,_=>0};
            if(signature.Key!=actual&&!(actual==6&&signature.Key==912))return 77;
            bool[]? usage=a.GetKeyUsage();if(usage!=null&&(usage.Length<=5||!usage[5]))return 32;
            return 0;
        }
        catch{Error();return 1;}
    }
    public static int X509_check_purpose(void* cert,int purpose,int ca)
    {
        try
        {
            var c=Get<CertState>(cert).Certificate.Value;if(purpose is not(1 or 2))throw new NotSupportedException();
            var eku=c.GetExtendedKeyUsage();string needed=purpose==1?"1.3.6.1.5.5.7.3.2":"1.3.6.1.5.5.7.3.1";
            if(eku!=null&&!eku.Contains(new DerObjectIdentifier(needed))&&!(purpose==2&&(eku.Contains(new DerObjectIdentifier("1.3.6.1.4.1.311.10.3.3"))||eku.Contains(new DerObjectIdentifier("2.16.840.1.113730.4.1")))))return 0;
            bool[]? usage=c.GetKeyUsage();
            var nsValue=ExtensionValue(c,new DerObjectIdentifier("2.16.840.1.113730.1.1"));int? ns=nsValue==null?null:DerBitString.GetInstance(nsValue).GetBytes().FirstOrDefault();
            if(ca!=0)
            {
                if(usage!=null&&(usage.Length<=5||!usage[5]))return 0;
                if(ExtensionValue(c,X509Extensions.BasicConstraints)!=null)return c.GetBasicConstraints()>=0?1:0;
                if(c.Version==1&&c.SubjectDN.Equivalent(c.IssuerDN,true))return 3;
                if(usage!=null)return 4;
                return ns is int value&&(value&4)!=0?5:0;
            }
            if(ns is int types&&(types&(purpose==1?128:64))==0)return 0;
            if(usage==null)return 1;
            return purpose==1?((usage.Length>0&&usage[0])||(usage.Length>4&&usage[4])?1:0):((usage.Length>0&&usage[0])||(usage.Length>2&&usage[2])||(usage.Length>4&&usage[4])?1:0);
        }
        catch{Error();return -1;}
    }
}
