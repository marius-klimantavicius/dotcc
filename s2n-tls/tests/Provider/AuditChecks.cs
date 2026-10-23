using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.S2nTls.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using BcCertificate=Org.BouncyCastle.X509.X509Certificate;
using BcExtensions=Org.BouncyCastle.Asn1.X509.X509Extensions;

internal static unsafe class AuditChecks
{
    private static void Check(bool result,string message){if(!result)throw new Exception(message);}
    private static void* Decode(byte[] der){fixed(byte* p=der){byte* cursor=p;void* result=OpenSslAbi.d2i_X509(null,&cursor,der.Length);Check(result!=null&&cursor==p+der.Length,"Certificate decode");return result;}}
    private static int IgnoreCrlTime(int ok,void* ctx)=>OpenSslAbi.X509_STORE_CTX_get_error(ctx) is 11 or 12 or 15 or 16?1:ok;
    internal static void Run(Action<string,Action> test,Func<string[],string> oracle,string artifacts,RSA rootKey,X509Certificate2 root,X509Certificate2 leaf,DateTimeOffset now)
    {
        string rootPath=Path.Combine(artifacts,"abi-root.pem"),leafPath=Path.Combine(artifacts,"abi-leaf.pem");
        var bcRoot=new X509CertificateParser().ReadCertificate(root.RawData);var bcLeaf=new X509CertificateParser().ReadCertificate(leaf.RawData);
        var signer=new Asn1SignatureFactory("SHA256WITHRSA",DotNetUtilities.GetRsaKeyPair(rootKey).Private);
        test("audit/crl-scope-critical-delta-reasons-retained-callback-native",()=>
        {
            int before=OpenSslAbi.LiveContexts;void* cert=Decode(leaf.RawData),anchor=Decode(root.RawData),store=OpenSslAbi.X509_STORE_new(),ctx=OpenSslAbi.X509_STORE_CTX_new();
            try
            {
                Check(OpenSslAbi.X509_STORE_add_cert(store,anchor)==1,"Add root");
                foreach(string variant in new[]{"user","user-noncritical","ca","attribute","indirect","reasons","delta","unknown-critical","critical-number","critical-entry","remove","named-mismatch","all","expired","expired-callback"})
                {
                    var generator=new X509V2CrlGenerator();generator.SetIssuerDN(bcRoot.SubjectDN);generator.SetThisUpdate(now.AddHours(-2).UtcDateTime);generator.SetNextUpdate(now.AddHours(variant.StartsWith("expired")?-1:2).UtcDateTime);
                    var scope=variant switch
                    {
                        "user" or "user-noncritical"=>new IssuingDistributionPoint(null,true,false,null,false,false),
                        "ca"=>new IssuingDistributionPoint(null,false,true,null,false,false),
                        "attribute"=>new IssuingDistributionPoint(null,false,false,null,false,true),
                        "indirect"=>new IssuingDistributionPoint(null,false,false,null,true,false),
                        "reasons"=>new IssuingDistributionPoint(null,false,false,new ReasonFlags(ReasonFlags.KeyCompromise),false,false),
                        "named-mismatch"=>new IssuingDistributionPoint(new DistributionPointName(new GeneralNames(new GeneralName(GeneralName.UniformResourceIdentifier,"https://provider.example/crl"))),false,false,null,false,false),
                        _=>null
                    };
                    if(scope!=null)generator.AddExtension(BcExtensions.IssuingDistributionPoint,variant!="user-noncritical",scope);
                    if(variant=="delta")generator.AddExtension(BcExtensions.DeltaCrlIndicator,true,DerInteger.ValueOf(1));
                    if(variant=="unknown-critical")generator.AddExtension(new DerObjectIdentifier("1.2.3.4.5"),true,DerNull.Instance);
                    if(variant=="critical-number")generator.AddExtension(BcExtensions.CrlNumber,true,DerInteger.ValueOf(1));
                    if(variant=="critical-entry")generator.AddCrlEntry(bcLeaf.SerialNumber,now.AddHours(-1).UtcDateTime,new X509Extensions(new Dictionary<DerObjectIdentifier,Org.BouncyCastle.Asn1.X509.X509Extension>{{BcExtensions.ReasonCode,new(true,new DerOctetString(new CrlReason(1)))}}));
                    if(variant=="remove")generator.AddCrlEntry(bcLeaf.SerialNumber,now.AddHours(-1).UtcDateTime,8);
                    byte[] der=generator.Generate(signer).GetEncoded();string path=Path.Combine(artifacts,"audit-"+variant+".crl.pem");File.WriteAllText(path,PemEncoding.WriteString("X509 CRL",der));void* crl;
                    fixed(byte* p=der){byte* cursor=p;crl=OpenSslAbi.d2i_X509_CRL(null,&cursor,der.Length);Check(crl!=null,"CRL decode "+variant);}
                    Check(OpenSslAbi.X509_STORE_CTX_init(ctx,store,cert,null)==1,"Context init");void* stack=OpenSslAbi.OPENSSL_sk_new_null();Check(OpenSslAbi.OPENSSL_sk_push(stack,crl)==1,"Push CRL");OpenSslAbi.X509_STORE_CTX_set0_crls(ctx,stack);OpenSslAbi.OPENSSL_sk_free(stack);OpenSslAbi.X509_CRL_free(crl);
                    ulong flags=variant=="all"?0x8000cUL:0x80004UL;OpenSslAbi.X509_STORE_CTX_set_flags(ctx,flags);OpenSslAbi.X509_STORE_CTX_set_time(ctx,0,now.ToUnixTimeSeconds());
                    bool callback=variant=="expired-callback";if(callback)OpenSslAbi.X509_STORE_CTX_set_verify_cb(ctx,&IgnoreCrlTime);
                    GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();int ok=OpenSslAbi.X509_verify_cert(ctx),error=OpenSslAbi.X509_STORE_CTX_get_error(ctx);
                    var command=new List<string>{"crl",leafPath,rootPath,path,now.ToUnixTimeSeconds().ToString(),flags.ToString()};if(callback)command.Add("ignore-time");
                    string native=oracle(command.ToArray());Check($"{ok} {error}"==native,$"CRL {variant}: managed {ok} {error}, native {native}");
                    if(callback){OpenSslAbi.X509_STORE_CTX_set_verify_cb(ctx,null);Check(OpenSslAbi.X509_verify_cert(ctx)==0,"Cleared CRL callback bypassed expiry");}
                }
                Check(OpenSslAbi.X509_STORE_CTX_init(ctx,store,cert,null)==1,"Reset context");OpenSslAbi.X509_STORE_CTX_set_flags(ctx,0x80000000);Check(OpenSslAbi.X509_verify_cert(ctx)==0,"Unsupported void flag was silently ignored");
            }
            finally{OpenSslAbi.X509_STORE_CTX_free(ctx);OpenSslAbi.X509_STORE_free(store);OpenSslAbi.X509_free(cert);OpenSslAbi.X509_free(anchor);}
            Check(OpenSslAbi.LiveContexts==before,"CRL audit leaked retained objects");
        });
        test("audit/x509-purpose-critical-anchor-and-legacy-signatures-native",()=>
        {
            int before=OpenSslAbi.LiveContexts;
            foreach(string variant in new[]{"agreement","any-eku","sgc","netscape-deny","netscape-critical","netscape-critical-deny","legacy-ca","md5","sha1","unknown-critical-leaf","unknown-critical-root"})
            {
                var generator=new X509V3CertificateGenerator();generator.SetIssuerDN(bcRoot.SubjectDN);generator.SetSubjectDN(new X509Name("CN=Audit "+variant));generator.SetSerialNumber(new Org.BouncyCastle.Math.BigInteger("123"));generator.SetNotBefore(now.AddHours(-1).UtcDateTime);generator.SetNotAfter(now.AddHours(1).UtcDateTime);generator.SetPublicKey(bcLeaf.GetPublicKey());
                if(variant=="agreement")generator.AddExtension(BcExtensions.KeyUsage,true,new KeyUsage(KeyUsage.KeyAgreement));
                if(variant=="any-eku")generator.AddExtension(BcExtensions.ExtendedKeyUsage,false,new ExtendedKeyUsage(KeyPurposeID.AnyExtendedKeyUsage));
                if(variant=="sgc")generator.AddExtension(BcExtensions.ExtendedKeyUsage,false,new DerSequence(new DerObjectIdentifier("1.3.6.1.4.1.311.10.3.3")));
                if(variant=="netscape-deny")generator.AddExtension(new DerObjectIdentifier("2.16.840.1.113730.1.1"),false,new DerBitString(new byte[]{0}));
                if(variant.StartsWith("netscape-critical"))generator.AddExtension(new DerObjectIdentifier("2.16.840.1.113730.1.1"),true,new DerBitString(new byte[]{variant=="netscape-critical"?(byte)0xc0:(byte)0}));
                if(variant=="legacy-ca")generator.AddExtension(BcExtensions.KeyUsage,true,new KeyUsage(KeyUsage.KeyCertSign));
                if(variant.StartsWith("unknown-critical"))generator.AddExtension(new DerObjectIdentifier("1.2.3.4.5"),true,DerNull.Instance);
                if(variant=="unknown-critical-root"){generator.SetSubjectDN(bcRoot.SubjectDN);generator.SetPublicKey(bcRoot.GetPublicKey());generator.AddExtension(BcExtensions.BasicConstraints,true,new BasicConstraints(true));}
                var signing=variant is "md5" or "sha1"?new Asn1SignatureFactory(variant+"WITHRSA",DotNetUtilities.GetRsaKeyPair(rootKey).Private):signer;
                var generated=generator.Generate(signing);byte[] der=generated.GetEncoded();string path=Path.Combine(artifacts,"audit-"+variant+".pem");File.WriteAllText(path,PemEncoding.WriteString("CERTIFICATE",der));void* cert=Decode(der);
                try
                {
                    foreach(int ca in new[]{0,1}){string result=$"{OpenSslAbi.X509_check_purpose(cert,1,ca)} {OpenSslAbi.X509_check_purpose(cert,2,ca)}";Check(result==oracle(["x509-purpose",path,ca.ToString()]),"Purpose "+variant+": "+result);}
                    if(variant is "md5" or "sha1" or "unknown-critical-leaf" or "unknown-critical-root" or "netscape-critical" or "netscape-critical-deny")
                    {
                        bool rootVariant=variant=="unknown-critical-root";void* trust=Decode(rootVariant?der:root.RawData),target=rootVariant?Decode(leaf.RawData):cert,store=OpenSslAbi.X509_STORE_new(),ctx=OpenSslAbi.X509_STORE_CTX_new();
                        try{Check(OpenSslAbi.X509_STORE_add_cert(store,trust)==1&&OpenSslAbi.X509_STORE_CTX_init(ctx,store,target,null)==1,"Verify setup");OpenSslAbi.X509_STORE_CTX_set_flags(ctx,0x80000);OpenSslAbi.X509_STORE_CTX_set_time(ctx,0,now.ToUnixTimeSeconds());int ok=OpenSslAbi.X509_verify_cert(ctx),error=OpenSslAbi.X509_STORE_CTX_get_error(ctx);string native=oracle(["x509-verify",rootVariant?leafPath:path,rootVariant?path:rootPath,now.ToUnixTimeSeconds().ToString(),"524288"]);Check($"{ok} {error}"==native,$"Verify {variant}: {ok} {error} != {native}");}
                        finally{OpenSslAbi.X509_STORE_CTX_free(ctx);OpenSslAbi.X509_STORE_free(store);OpenSslAbi.X509_free(trust);if(rootVariant)OpenSslAbi.X509_free(target);}
                    }
                }
                finally{OpenSslAbi.X509_free(cert);}
            }
            Check(OpenSslAbi.LiveContexts==before,"Certificate audit leaked objects");
        });
        test("audit/asn1-malformed-cursors-and-ocsp-empty-delegated-native",()=>
        {
            int before=OpenSslAbi.LiveContexts;
            foreach(byte[] malformed in new byte[][]{[],[0x30],[0x30,0x82,0xff],[0x30,0],[0x04,1,0]})fixed(byte* p=malformed)
            {
                byte* cursor=p;Check(OpenSslAbi.d2i_X509(null,&cursor,malformed.Length)==null&&cursor==p,"Malformed X509 advanced cursor");cursor=p;Check(OpenSslAbi.d2i_X509_CRL(null,&cursor,malformed.Length)==null&&cursor==p,"Malformed CRL advanced cursor");cursor=p;Check(OpenSslAbi.d2i_OCSP_RESPONSE(null,&cursor,malformed.Length)==null&&cursor==p,"Malformed OCSP advanced cursor");cursor=p;Check(OpenSslAbi.d2i_AutoPrivateKey(null,&cursor,malformed.Length)==null&&cursor==p,"Malformed private key advanced cursor");
            }
            using var delegatedKey=RSA.Create(1024);var bcDelegated=DotNetUtilities.GetRsaKeyPair(delegatedKey);
            foreach(string variant in new[]{"empty","delegate","delegate-no-eku","delegate-wrong-eku"})
            {
                var certGenerator=new X509V3CertificateGenerator();certGenerator.SetIssuerDN(bcRoot.SubjectDN);certGenerator.SetSubjectDN(new X509Name("CN=OCSP Delegate"));certGenerator.SetSerialNumber(new Org.BouncyCastle.Math.BigInteger("456"));certGenerator.SetNotBefore(now.AddHours(-1).UtcDateTime);certGenerator.SetNotAfter(now.AddHours(1).UtcDateTime);certGenerator.SetPublicKey(bcDelegated.Public);
                if(variant=="delegate")certGenerator.AddExtension(BcExtensions.ExtendedKeyUsage,false,new ExtendedKeyUsage(KeyPurposeID.id_kp_OCSPSigning));
                if(variant=="delegate-wrong-eku")certGenerator.AddExtension(BcExtensions.ExtendedKeyUsage,false,new ExtendedKeyUsage(KeyPurposeID.id_kp_serverAuth));
                BcCertificate delegated=certGenerator.Generate(signer);var generator=new BasicOcspRespGenerator(new RespID(variant=="empty"?bcRoot.SubjectDN:delegated.SubjectDN));
                if(variant!="empty")generator.AddResponse(new CertificateID(CertificateID.DigestSha1,bcRoot,bcLeaf.SerialNumber),CertificateStatus.Good);
                var basic=generator.Generate(variant=="empty"?signer:new Asn1SignatureFactory("SHA256WITHRSA",bcDelegated.Private),variant=="empty"?[bcRoot]:[delegated,bcRoot],now.UtcDateTime);
                byte[] der=new OCSPRespGenerator().Generate(0,basic).GetEncoded();string path=Path.Combine(artifacts,"audit-"+variant+".ocsp.der");File.WriteAllBytes(path,der);void* response,store=OpenSslAbi.X509_STORE_new(),anchor=Decode(root.RawData);
                fixed(byte* p=der){byte* cursor=p;response=OpenSslAbi.d2i_OCSP_RESPONSE(null,&cursor,der.Length);Check(response!=null,"OCSP parse "+variant);}
                try{Check(OpenSslAbi.X509_STORE_add_cert(store,anchor)==1,"OCSP trust");void* handle=OpenSslAbi.OCSP_response_get1_basic(response);try{int ok=OpenSslAbi.OCSP_basic_verify(handle,null,store,0);Check(ok.ToString()==oracle(["ocsp-basic",rootPath,path]),"OCSP authorization "+variant);Check(OpenSslAbi.OCSP_basic_verify(handle,null,store,0x80000000)==0,"Unsupported OCSP flags ignored");}finally{OpenSslAbi.OCSP_BASICRESP_free(handle);}}
                finally{OpenSslAbi.OCSP_RESPONSE_free(response);OpenSslAbi.X509_STORE_free(store);OpenSslAbi.X509_free(anchor);}
            }
            Check(OpenSslAbi.LiveContexts==before,"Malformed/OCSP audit leaked objects");
        });
    }
}
