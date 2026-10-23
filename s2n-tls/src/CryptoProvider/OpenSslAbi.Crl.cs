using System.Formats.Asn1;
using System.Text;
using Org.BouncyCastle.X509;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private sealed class CrlState(X509Crl value) : IDisposable
    {
        public readonly X509Crl Value=value;
        public readonly Dictionary<string,nint> Children=new();
        private int references=1;
        public CrlState Retain(){Interlocked.Increment(ref references);return this;}
        public nint Borrow(string key,Func<nint> factory){lock(Children){if(Children.TryGetValue(key,out var value))return value;value=factory();Children.Add(key,value);return value;}}
        public void Dispose(){if(Interlocked.Decrement(ref references)!=0)return;foreach(var value in Children.Values)Remove((void*)value);Children.Clear();}
    }
    public static void* d2i_X509_CRL(void** output,byte** input,long length)
    {
        CrlState? crl=null;
        try
        {
            if(input==null||length<0)throw new ArgumentException();var reader=new AsnReader(Input(*input,(ulong)length).ToArray(),AsnEncodingRules.DER);var encoded=reader.ReadEncodedValue();crl=new CrlState(new X509CrlParser().ReadCrl(encoded.ToArray())??throw new ArgumentException("DER value is not a CRL."));void* handle=Add(crl);crl=null;
            if(output!=null){if(*output!=null)Remove(*output);*output=handle;}*input+=encoded.Length;return handle;
        }
        catch{crl?.Dispose();Error();return null;}
    }
    public static void X509_CRL_free(void* crl){try{Remove(crl);}catch{Error();}}
    public static void* X509_CRL_get_issuer(void* crl)
    {try{var c=Get<CrlState>(crl);return (void*)c.Borrow("issuer",()=>(nint)Add(new NameState(c.Value.IssuerDN)));}catch{Error();return null;}}
    private static nint TimeValue(DateTime date)
    {string text=date.ToUniversalTime().ToString("yyyyMMddHHmmss'Z'",System.Globalization.CultureInfo.InvariantCulture);return Asn(24,Encoding.ASCII.GetBytes(text),text,date.ToUniversalTime());}
    public static void* X509_CRL_get_lastUpdate(void* crl)
    {try{var c=Get<CrlState>(crl);return (void*)c.Borrow("last",()=>TimeValue(c.Value.ThisUpdate));}catch{Error();return null;}}
    public static void* X509_CRL_get_nextUpdate(void* crl)
    {try{var c=Get<CrlState>(crl);return c.Value.NextUpdate is DateTime next?(void*)c.Borrow("next",()=>TimeValue(next)):null;}catch{Error();return null;}}
}
