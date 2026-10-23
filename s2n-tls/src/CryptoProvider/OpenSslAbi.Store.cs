using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private const ulong CrlCheck=4,CrlCheckAll=8,PartialChain=0x80000,NoCheckTime=0x200000;
    private sealed class StoreState : IDisposable
    {
        public readonly List<CertState> Certificates=new();
        public nint Objects;
        private int references=1;
        public StoreState Retain(){Interlocked.Increment(ref references);return this;}
        public void Add(CertState cert)
        {
            if(Certificates.Any(x=>x.Certificate.Value.Equals(cert.Certificate.Value)))return;
            Certificates.Add(cert.Retain());if(Objects!=0)Get<StackState>((void*)Objects).Values.Add((nint)OpenSslAbi.Add(new OwnedX509Object((nint)OpenSslAbi.Add(cert.Retain()))));
        }
        public void Dispose(){if(Interlocked.Decrement(ref references)!=0)return;if(Objects!=0){foreach(var value in Get<StackState>((void*)Objects).Values)Remove((void*)value);Remove((void*)Objects);}foreach(var cert in Certificates)cert.Dispose();Certificates.Clear();}
    }
    private sealed class VerifyParameters {public int Depth=100;public ulong Flags;public DateTime? Time;public bool Invalid;}
    private sealed class StoreContext : IDisposable
    {
        public StoreState? Store;
        public CertState? Leaf;
        public readonly List<CertState> Wire=new();
        public readonly List<CrlState> Crls=new();
        public nint Chain,Parameters;
        public int Error,CallbackError;
        public delegate*<int,void*,int> Callback;
        public readonly VerifyParameters Param=new();
        public void Clear()
        {
            if(Chain!=0){foreach(nint cert in Get<StackState>((void*)Chain).Values)Remove((void*)cert);Remove((void*)Chain);Chain=0;}
            Store?.Dispose();Store=null;Leaf?.Dispose();Leaf=null;foreach(var cert in Wire)cert.Dispose();Wire.Clear();foreach(var crl in Crls)crl.Dispose();Crls.Clear();Error=0;
        }
        public void Dispose(){Clear();if(Parameters!=0)Remove((void*)Parameters);Parameters=0;Callback=null;}
    }
    public static void* X509_STORE_new(){try{return Add(new StoreState());}catch{Error();return null;}}
    public static void X509_STORE_free(void* store){try{Remove(store);}catch{Error();}}
    public static int X509_STORE_add_cert(void* store,void* cert){try{Get<StoreState>(store).Add(Get<CertState>(cert));return 1;}catch{Error();return 0;}}
    public static void* X509_STORE_get0_objects(void* store)
    {
        try
        {
            var s=Get<StoreState>(store);if(s.Objects==0)
            {
                s.Objects=(nint)OPENSSL_sk_new_null();var values=Get<StackState>((void*)s.Objects).Values;
                foreach(var cert in s.Certificates)
                {
                    nint handle=(nint)Add(cert.Retain());values.Add((nint)Add(new OwnedX509Object(handle)));
                }
            }
            return (void*)s.Objects;
        }
        catch{Error();return null;}
    }
    private sealed class OwnedX509Object(nint cert) : IDisposable
    {public readonly nint Certificate=cert;public void Dispose()=>Remove((void*)Certificate);}
    public static void* X509_OBJECT_get0_X509(void* value){try{return (void*)Get<OwnedX509Object>(value).Certificate;}catch{Error();return null;}}
    private static void LoadPem(StoreState store,string text)
    {
        ReadOnlySpan<char> remaining=text;int found=0;
        while(PemEncoding.TryFind(remaining,out var item))
        {
            if(remaining[item.Label].SequenceEqual("CERTIFICATE"))
            {
                byte[] der=Convert.FromBase64String(remaining[item.Base64Data].ToString());var cert=new CertState(Certificate.ParseDer(der,out int count));
                try{if(count!=der.Length)throw new CryptographicException();store.Add(cert);found++;}finally{cert.Dispose();}
            }
            remaining=remaining[item.Location.End..];
        }
        if(found==0)throw new CryptographicException("No CA certificates in PEM input.");
    }
    public static int X509_STORE_load_locations(void* store,byte* file,byte* directory)
    {
        try
        {
            var s=Get<StoreState>(store);if(file==null&&directory==null)throw new ArgumentException();
            if(file!=null)LoadPem(s,File.ReadAllText(Marshal.PtrToStringUTF8((nint)file)??throw new ArgumentException()));
            if(directory!=null)
            {
                string path=Marshal.PtrToStringUTF8((nint)directory)??throw new ArgumentException();
                foreach(string entry in Directory.EnumerateFiles(path))
                {try{LoadPem(s,File.ReadAllText(entry));}catch(CryptographicException){}catch(IOException){}}
            }
            return 1;
        }
        catch{Error();return 0;}
    }
    public static int X509_STORE_set_default_paths(void* store)
    {
        try
        {
            var s=Get<StoreState>(store);using var system=new X509Store(StoreName.Root,StoreLocation.CurrentUser);system.Open(OpenFlags.ReadOnly);
            foreach(var item in system.Certificates)
            {using(item){var cert=new CertState(Certificate.ParseDer(item.RawData,out _));try{s.Add(cert);}finally{cert.Dispose();}}}
            return 1;
        }
        catch{Error();return 0;}
    }
    public static void* X509_STORE_CTX_new()
    {
        StoreContext? state=null;
        try{state=new StoreContext();state.Parameters=(nint)Add(state.Param);void* result=Add(state);state=null;return result;}catch{state?.Dispose();Error();return null;}
    }
    public static void X509_STORE_CTX_free(void* ctx){try{Remove(ctx);}catch{Error();}}
    public static int X509_STORE_CTX_init(void* ctx,void* store,void* leaf,void* untrusted)
    {
        try
        {
            var c=Get<StoreContext>(ctx);c.Clear();c.Param.Flags=0;c.Param.Time=null;c.Param.Depth=100;c.Param.Invalid=false;c.Callback=null;c.Store=Get<StoreState>(store).Retain();c.Leaf=Get<CertState>(leaf).Retain();
            if(untrusted!=null)foreach(nint handle in Get<StackState>(untrusted).Values)c.Wire.Add(Get<CertState>((void*)handle).Retain());return 1;
        }
        catch{Error();return 0;}
    }
    public static void* X509_STORE_CTX_get0_param(void* ctx){try{return (void*)Get<StoreContext>(ctx).Parameters;}catch{Error();return null;}}
    public static int X509_STORE_CTX_get_error(void* ctx){try{return Get<StoreContext>(ctx).Error;}catch{Error();return 1;}}
    public static void* X509_STORE_CTX_get0_chain(void* ctx){try{return (void*)Get<StoreContext>(ctx).Chain;}catch{Error();return null;}}
    public static void X509_VERIFY_PARAM_set_depth(void* param,int depth){try{if(depth<0)throw new ArgumentException();Get<VerifyParameters>(param).Depth=depth;}catch{Error();}}
    public static int X509_VERIFY_PARAM_set_flags(void* param,ulong flags)
    {try{var p=Get<VerifyParameters>(param);if((flags&~(CrlCheck|CrlCheckAll|PartialChain|NoCheckTime))!=0){p.Invalid=true;throw new NotSupportedException("Unselected verification flag.");}p.Flags|=flags;return 1;}catch{Error();return 0;}}
    public static void X509_STORE_CTX_set_flags(void* ctx,ulong flags)
    {try{if(X509_VERIFY_PARAM_set_flags((void*)Get<StoreContext>(ctx).Parameters,flags)!=1)throw new ArgumentException();}catch{Error();}}
    public static void X509_STORE_CTX_set_time(void* ctx,ulong flags,long seconds)
    {try{var p=Get<StoreContext>(ctx).Param;p.Time=DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;p.Flags&=~NoCheckTime;}catch{Error();}}
    public static void X509_STORE_CTX_set_verify_cb(void* ctx,delegate*<int,void*,int> callback)
    {try{Get<StoreContext>(ctx).Callback=callback;}catch{Error();}}
    public static void X509_STORE_CTX_set0_crls(void* ctx,void* crls)
    {
        try{var c=Get<StoreContext>(ctx);foreach(var crl in c.Crls)crl.Dispose();c.Crls.Clear();if(crls!=null)foreach(nint handle in Get<StackState>(crls).Values)c.Crls.Add(Get<CrlState>((void*)handle).Retain());}catch{Error();}
    }
    private sealed class ValidatingCertificate : Org.BouncyCastle.X509.X509Certificate
    {
        private readonly StoreContext context;
        private readonly nint contextPointer;
        public ValidatingCertificate(Org.BouncyCastle.X509.X509Certificate source,StoreContext context,void* pointer):base(source.CertificateStructure){this.context=context;contextPointer=(nint)pointer;}
        public override void CheckValidity(DateTime date)
        {
            if((context.Param.Flags&NoCheckTime)!=0)return;
            int error=date<NotBefore?9:date>NotAfter?10:0;
            if(error!=0)
            {
                context.Error=error;if(context.Callback!=null&&context.Callback(0,(void*)contextPointer)==1){context.CallbackError=error;context.Error=0;return;}
            }
            base.CheckValidity(date);
        }
    }
    private sealed class NativePurposeExtensions : PkixCertPathChecker
    {
        private const string Netscape="2.16.840.1.113730.1.1";
        public override void Init(bool forward){}
        public override bool IsForwardCheckingSupported()=>true;
        public override ISet<string> GetSupportedExtensions()=>new HashSet<string>{Netscape,"2.5.29.37","2.5.29.31"};
        public override void Check(Org.BouncyCastle.X509.X509Certificate certificate,ISet<string> unresolved)
        {
            // OpenSSL recognizes this legacy BIT STRING. Its SSL client/server
            // and CA bits are enforced by X509_check_purpose when translated
            // s2n checks the complete chain's intent, after path verification.
            if(unresolved.Contains(Netscape))
            {_=Org.BouncyCastle.Asn1.DerBitString.GetInstance(ExtensionValue(certificate,new Org.BouncyCastle.Asn1.DerObjectIdentifier(Netscape)));unresolved.Remove(Netscape);}
            // BC handles these on the leaf, but leaves them unresolved on
            // intermediates. Decode the actual values; translated s2n checks
            // EKU on every validated certificate, and VerifyCrls uses each DP.
            if(unresolved.Contains("2.5.29.37"))
            {_=certificate.GetExtendedKeyUsage();unresolved.Remove("2.5.29.37");}
            if(unresolved.Contains("2.5.29.31"))
            {_=Org.BouncyCastle.Asn1.X509.CrlDistPoint.GetInstance(ExtensionValue(certificate,Org.BouncyCastle.Asn1.X509.X509Extensions.CrlDistributionPoints)).GetDistributionPoints();unresolved.Remove("2.5.29.31");}
        }
    }
    private static bool BuildChain(CertState current,StoreContext context,List<CertState> path,HashSet<CertState> seen)
    {
        if(path.Count>context.Param.Depth+1||!seen.Add(current))return false;path.Add(current);
        var certificate=current.Certificate.Value;
        if(context.Store!.Certificates.Any(x=>x.Certificate.Value.Equals(certificate)))
        {
            if((context.Param.Flags&PartialChain)!=0||certificate.SubjectDN.Equivalent(certificate.IssuerDN,true))return true;
        }
        foreach(var issuer in context.Wire.Concat(context.Store.Certificates).Distinct())
        {
            if(seen.Contains(issuer)||!certificate.IssuerDN.Equivalent(issuer.Certificate.Value.SubjectDN,true))continue;
            if(!current.Certificate.VerifySignature(issuer.Certificate))continue;
            if(BuildChain(issuer,context,path,seen))return true;
        }
        path.RemoveAt(path.Count-1);seen.Remove(current);return false;
    }
    private static int VerificationError(StoreContext context,void* pointer,int error)
    {context.Error=error;if(context.Callback!=null)context.Callback(0,pointer);Error();return 0;}
    public static int X509_verify_cert(void* ctx)
    {
        try
        {
            var c=Get<StoreContext>(ctx);if(c.Store==null||c.Leaf==null||c.Param.Invalid)throw new ArgumentException();c.Error=c.CallbackError=0;
            var path=new List<CertState>();if(!BuildChain(c.Leaf,c,path,new HashSet<CertState>()))return VerificationError(c,ctx,c.Wire.Count>c.Param.Depth?22:20);
            DateTime time=c.Param.Time??DateTime.UtcNow;
            // Native checks critical extensions on the complete chain, including
            // its trust anchor. PKIX validation alone omits the anchor.
            foreach(var item in path)if(HasUnhandledCertificateCriticalExtension(item.Certificate.Value))return VerificationError(c,ctx,34);
            // Native reports INVALID_CA before generic signature/path errors.
            // Trust anchors are not ordinary intermediate certificates.
            foreach(var item in path.Skip(1).Take(Math.Max(0,path.Count-2)))
            {
                var intermediate=item.Certificate.Value;bool[]? usage=intermediate.GetKeyUsage();
                if(intermediate.GetBasicConstraints()<0||usage!=null&&(usage.Length<=5||!usage[5]))return VerificationError(c,ctx,24);
            }
            if(path.Count==1)
            {new ValidatingCertificate(path[0].Certificate.Value,c,ctx).CheckValidity(time);}
            else
            {
                var trust=new HashSet<TrustAnchor>{new(new ValidatingCertificate(path[^1].Certificate.Value,c,ctx),null)};
                var options=new PkixParameters(trust){IsRevocationEnabled=false,Date=time};
                options.AddCertPathChecker(new NativePurposeExtensions());
                try{new PkixCertPathValidator().Validate(new PkixCertPath(path.Take(path.Count-1).Select(x=>(Org.BouncyCastle.X509.X509Certificate)new ValidatingCertificate(x.Certificate.Value,c,ctx)).ToList()),options);}
                catch(PkixCertPathValidatorException error)
                {return VerificationError(c,ctx,c.Error!=0?c.Error:error.Message.Contains("critical",StringComparison.OrdinalIgnoreCase)?34:7);}
                // OpenSSL also checks the trust anchor's time unless explicitly disabled.
                new ValidatingCertificate(path[^1].Certificate.Value,c,ctx).CheckValidity(time);
            }
            if((c.Param.Flags&CrlCheck)!=0&& !VerifyCrls(c,ctx,path,time))return 0;
            if(c.Callback!=null)foreach(var ignored in path)if(c.Callback(1,ctx)!=1)return VerificationError(c,ctx,50);
            if(c.Chain!=0){foreach(nint handle in Get<StackState>((void*)c.Chain).Values)Remove((void*)handle);Remove((void*)c.Chain);}
            c.Chain=(nint)OPENSSL_sk_new_null();foreach(var cert in path)Get<StackState>((void*)c.Chain).Values.Add((nint)Add(cert.Retain()));c.Error=c.CallbackError;return 1;
        }
        catch(Org.BouncyCastle.Security.Certificates.CertificateExpiredException){return VerificationError(Get<StoreContext>(ctx),ctx,10);}
        catch(Org.BouncyCastle.Security.Certificates.CertificateNotYetValidException){return VerificationError(Get<StoreContext>(ctx),ctx,9);}
        catch{try{return VerificationError(Get<StoreContext>(ctx),ctx,1);}catch{Error();return 0;}}
    }
}
