using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Managed.S2nTls.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

internal static unsafe class CertificateChecks
{
    private static void Check(bool value,string message="X509 ABI assertion failed."){if(!value)throw new Exception(message);}
    private static void* Decode(byte[] der){fixed(byte* input=der){byte* cursor=input;void* cert=OpenSslAbi.d2i_X509(null,&cursor,der.Length);Check(cert!=null&&cursor==input+der.Length);return cert;}}
    [StructLayout(LayoutKind.Sequential)]private struct GeneralName{public int Type;public void* Value;}
    internal static void Run(Action<string,Action> test,Func<string[],string> oracle,string artifacts)
    {
        using var rootKey=RSA.Create(2048);var rootRequest=new CertificateRequest("CN=MiXeD   Provider Root,O=Provider",rootKey,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
        rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true,false,0,true));rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign|X509KeyUsageFlags.CrlSign,true));
        var now=DateTimeOffset.UtcNow;using var root=rootRequest.CreateSelfSigned(now.AddDays(-2),now.AddDays(5));using var key=RSA.Create(1024);
        var request=new CertificateRequest("CN=MiXeD   Provider Leaf,O=Provider",key,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false,false,0,true));var san=new SubjectAlternativeNameBuilder();san.AddDnsName("provider.example");san.AddIpAddress(System.Net.IPAddress.Loopback);request.CertificateExtensions.Add(san.Build());
        using var leaf=request.Create(root,now.AddHours(-1),now.AddDays(1),[1,9,7,3]);string leafPath=Path.Combine(artifacts,"abi-leaf.pem"),rootPath=Path.Combine(artifacts,"abi-root.pem");File.WriteAllText(leafPath,leaf.ExportCertificatePem());File.WriteAllText(rootPath,root.ExportCertificatePem());
        test("abi/x509-native-nids-names-der-san-layout",()=>
        {
            int before=OpenSslAbi.LiveContexts;void* cert=Decode(leaf.RawData);
            try
            {
                string actual=$"{OpenSslAbi.X509_NAME_hash(OpenSslAbi.X509_get_subject_name(cert))} {OpenSslAbi.X509_issuer_name_hash(cert)} {OpenSslAbi.X509_get_signature_nid(cert)} {OpenSslAbi.X509_get_ext_count(cert)} {OpenSslAbi.X509_check_purpose(cert,1,0)} {OpenSslAbi.X509_check_purpose(cert,2,0)}";
                Check(actual==oracle(["x509-meta",leafPath]),$"Native metadata differs: {actual}");
                int n=OpenSslAbi.i2d_X509(cert,null);Check(n==leaf.RawData.Length);byte* allocated=null;Check(OpenSslAbi.i2d_X509(cert,&allocated)==n);try{Check(new ReadOnlySpan<byte>(allocated,n).SequenceEqual(leaf.RawData));}finally{OpenSslAbi.CRYPTO_free(allocated,null,0);}
                void* subject=OpenSslAbi.X509_get_subject_name(cert);int index=OpenSslAbi.X509_NAME_get_index_by_NID(subject,13,-1);Check(index>=0);void* entry=OpenSslAbi.X509_NAME_get_entry(subject,index);byte* text=null;
                int textLength=OpenSslAbi.ASN1_STRING_to_UTF8(&text,OpenSslAbi.X509_NAME_ENTRY_get_data(entry));try{Check(Encoding.UTF8.GetString(new ReadOnlySpan<byte>(text,textLength))=="MiXeD   Provider Leaf");}finally{OpenSslAbi.CRYPTO_free(text,null,0);}
                void* names=OpenSslAbi.X509_get_ext_d2i(cert,85,null,null);Check(names!=null&&OpenSslAbi.OPENSSL_sk_num(names)==2);
                try
                {
                    var dns=(GeneralName*)OpenSslAbi.OPENSSL_sk_value(names,0);Check(dns->Type==2);Check(Encoding.ASCII.GetString(new ReadOnlySpan<byte>(OpenSslAbi.ASN1_STRING_get0_data(dns->Value),OpenSslAbi.ASN1_STRING_length(dns->Value)))=="provider.example");
                    var ip=(GeneralName*)OpenSslAbi.OPENSSL_sk_value(names,1);Check(ip->Type==7&&new ReadOnlySpan<byte>(OpenSslAbi.ASN1_STRING_get0_data(ip->Value),4).SequenceEqual(new byte[]{127,0,0,1}));
                }
                finally{OpenSslAbi.GENERAL_NAMES_free(names);}
                int hash=0,pkey=0;Check(OpenSslAbi.OBJ_find_sigid_algs(OpenSslAbi.X509_get_signature_nid(cert),&hash,&pkey)==1&&hash==672&&pkey==6);
            }
            finally{OpenSslAbi.X509_free(cert);}
            Check(OpenSslAbi.LiveContexts==before,"Certificate getters leaked contexts.");
        });
        test("abi/x509-store-trust-time-disable-native",()=>
        {
            int before=OpenSslAbi.LiveContexts;void* cert=Decode(leaf.RawData),anchor=Decode(root.RawData),store=OpenSslAbi.X509_STORE_new(),ctx=OpenSslAbi.X509_STORE_CTX_new();
            try
            {
                Check(OpenSslAbi.X509_STORE_add_cert(store,anchor)==1);void* objects=OpenSslAbi.X509_STORE_get0_objects(store);Check(OpenSslAbi.OPENSSL_sk_num(objects)==1);Check(OpenSslAbi.X509_NAME_cmp(OpenSslAbi.X509_get_subject_name(OpenSslAbi.X509_OBJECT_get0_X509(OpenSslAbi.OPENSSL_sk_value(objects,0))),OpenSslAbi.X509_get_subject_name(anchor))==0);
                foreach(var variant in new[]{(now,0x80000UL),(now.AddDays(2),0x80000UL),(now.AddDays(-3),0x80000UL),(now.AddDays(9),0x280000UL)})
                {
                    Check(OpenSslAbi.X509_STORE_CTX_init(ctx,store,cert,null)==1);OpenSslAbi.X509_STORE_CTX_set_flags(ctx,variant.Item2);
                    if((variant.Item2&0x200000)==0)OpenSslAbi.X509_STORE_CTX_set_time(ctx,0,variant.Item1.ToUnixTimeSeconds());
                    int result=OpenSslAbi.X509_verify_cert(ctx),error=OpenSslAbi.X509_STORE_CTX_get_error(ctx);
                    string native=oracle(["x509-verify",leafPath,rootPath,variant.Item1.ToUnixTimeSeconds().ToString(),variant.Item2.ToString()]);Check($"{result} {error}"==native,$"Native validation differs: {result} {error} != {native}");
                    if(result==1)Check(OpenSslAbi.OPENSSL_sk_num(OpenSslAbi.X509_STORE_CTX_get0_chain(ctx))==2);
                }
                // An empty trust store cannot validate the same correctly signed leaf.
                void* empty=OpenSslAbi.X509_STORE_new();try{Check(OpenSslAbi.X509_STORE_CTX_init(ctx,empty,cert,null)==1&&OpenSslAbi.X509_verify_cert(ctx)==0);}finally{OpenSslAbi.X509_STORE_free(empty);}
            }
            finally{OpenSslAbi.X509_STORE_CTX_free(ctx);OpenSslAbi.X509_STORE_free(store);OpenSslAbi.X509_free(cert);OpenSslAbi.X509_free(anchor);}
            Check(OpenSslAbi.LiveContexts==before,"Trust store/context leaked references.");
        });
        test("abi/x509-callback-time-error-gc-clear",()=>
        {
            int before=OpenSslAbi.LiveContexts;void* cert=Decode(leaf.RawData),anchor=Decode(root.RawData),store=OpenSslAbi.X509_STORE_new(),ctx=OpenSslAbi.X509_STORE_CTX_new();
            try
            {
                Check(OpenSslAbi.X509_STORE_add_cert(store,anchor)==1);Check(OpenSslAbi.X509_STORE_CTX_init(ctx,store,cert,null)==1);OpenSslAbi.X509_STORE_CTX_set_flags(ctx,0x80000);OpenSslAbi.X509_STORE_CTX_set_time(ctx,0,now.AddDays(2).ToUnixTimeSeconds());
                VerifyCallback.Calls=0;OpenSslAbi.X509_STORE_CTX_set_verify_cb(ctx,&VerifyCallback.IgnoreTime);GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();Check(OpenSslAbi.X509_verify_cert(ctx)==1&&VerifyCallback.Calls>0,"Retained verification callback did not override only time errors.");
                OpenSslAbi.X509_STORE_CTX_set_verify_cb(ctx,null);Check(OpenSslAbi.X509_verify_cert(ctx)==0&&OpenSslAbi.X509_STORE_CTX_get_error(ctx)==10,"Cleared callback still bypassed expiry.");
            }
            finally{OpenSslAbi.X509_STORE_CTX_free(ctx);OpenSslAbi.X509_STORE_free(store);OpenSslAbi.X509_free(cert);OpenSslAbi.X509_free(anchor);}
            Check(OpenSslAbi.LiveContexts==before,"Verification callback lifecycle leaked contexts.");
        });
        var bcRoot=new X509CertificateParser().ReadCertificate(root.RawData);var bcLeaf=new X509CertificateParser().ReadCertificate(leaf.RawData);var signer=new Asn1SignatureFactory("SHA256WITHRSA",DotNetUtilities.GetRsaKeyPair(rootKey).Private);
        test("abi/crl-native-good-revoked-bad-signature",()=>
        {
            int before=OpenSslAbi.LiveContexts;void* cert=Decode(leaf.RawData),anchor=Decode(root.RawData),store=OpenSslAbi.X509_STORE_new(),ctx=OpenSslAbi.X509_STORE_CTX_new(),crls=OpenSslAbi.OPENSSL_sk_new_null();
            try
            {
                Check(OpenSslAbi.X509_STORE_add_cert(store,anchor)==1);
                foreach(bool revoked in new[]{false,true})
                {
                    var generator=new X509V2CrlGenerator();generator.SetIssuerDN(bcRoot.SubjectDN);generator.SetThisUpdate(now.AddHours(-1).UtcDateTime);generator.SetNextUpdate(now.AddDays(1).UtcDateTime);if(revoked)generator.AddCrlEntry(bcLeaf.SerialNumber,now.AddMinutes(-10).UtcDateTime,1);
                    byte[] der=generator.Generate(signer).GetEncoded();string path=Path.Combine(artifacts,$"abi-{revoked}.crl.pem");File.WriteAllText(path,PemEncoding.WriteString("X509 CRL",der));void* crl;
                    fixed(byte* d=der){byte* cursor=d;crl=OpenSslAbi.d2i_X509_CRL(null,&cursor,der.Length);Check(crl!=null&&cursor==d+der.Length);}
                    try
                    {
                        Check(OpenSslAbi.X509_STORE_CTX_init(ctx,store,cert,null)==1);void* one=OpenSslAbi.OPENSSL_sk_new_null();try{Check(OpenSslAbi.OPENSSL_sk_push(one,crl)==1);OpenSslAbi.X509_STORE_CTX_set0_crls(ctx,one);}finally{OpenSslAbi.OPENSSL_sk_free(one);}
                        OpenSslAbi.X509_STORE_CTX_set_flags(ctx,0x80004);OpenSslAbi.X509_STORE_CTX_set_time(ctx,0,now.ToUnixTimeSeconds());int result=OpenSslAbi.X509_verify_cert(ctx),error=OpenSslAbi.X509_STORE_CTX_get_error(ctx);Check($"{result} {error}"==oracle(["crl",leafPath,rootPath,path,now.ToUnixTimeSeconds().ToString()]),"Native CRL validation differs.");
                        Check(OpenSslAbi.X509_cmp_time(OpenSslAbi.X509_CRL_get_lastUpdate(crl),null)<0&&OpenSslAbi.X509_cmp_time(OpenSslAbi.X509_CRL_get_nextUpdate(crl),null)>0);
                    }
                    finally{OpenSslAbi.X509_CRL_free(crl);}
                }
                // A cryptographically invalid CRL is parsed, then rejected by verification.
                var bad=new X509V2CrlGenerator();bad.SetIssuerDN(bcRoot.SubjectDN);bad.SetThisUpdate(now.AddHours(-1).UtcDateTime);bad.SetNextUpdate(now.AddDays(1).UtcDateTime);byte[] invalid=bad.Generate(signer).GetEncoded();invalid[^1]^=1;void* badCrl;
                fixed(byte* d=invalid){byte* cursor=d;badCrl=OpenSslAbi.d2i_X509_CRL(null,&cursor,invalid.Length);Check(badCrl!=null);}
                try{Check(OpenSslAbi.X509_STORE_CTX_init(ctx,store,cert,null)==1);OpenSslAbi.OPENSSL_sk_push(crls,badCrl);OpenSslAbi.X509_STORE_CTX_set0_crls(ctx,crls);OpenSslAbi.X509_STORE_CTX_set_flags(ctx,0x80004);Check(OpenSslAbi.X509_verify_cert(ctx)==0&&OpenSslAbi.X509_STORE_CTX_get_error(ctx)==8);}finally{OpenSslAbi.X509_CRL_free(badCrl);}
            }
            finally{OpenSslAbi.OPENSSL_sk_free(crls);OpenSslAbi.X509_STORE_CTX_free(ctx);OpenSslAbi.X509_STORE_free(store);OpenSslAbi.X509_free(cert);OpenSslAbi.X509_free(anchor);}
            Check(OpenSslAbi.LiveContexts==before,"CRL operations leaked references.");
        });
        test("abi/ocsp-native-good-revoked-unknown-reject-signature",()=>
        {
            int before=OpenSslAbi.LiveContexts;void* cert=Decode(leaf.RawData),anchor=Decode(root.RawData),store=OpenSslAbi.X509_STORE_new(),chain=OpenSslAbi.OPENSSL_sk_new_null();
            try
            {
                Check(OpenSslAbi.X509_STORE_add_cert(store,anchor)==1);OpenSslAbi.OPENSSL_sk_push(chain,cert);OpenSslAbi.OPENSSL_sk_push(chain,anchor);
                foreach(int state in new[]{0,1,2})
                {
                    var generator=new BasicOcspRespGenerator(new RespID(bcRoot.SubjectDN));generator.AddResponse(new CertificateID(CertificateID.DigestSha1,bcRoot,bcLeaf.SerialNumber),state==0?CertificateStatus.Good:state==1?new RevokedStatus(now.AddHours(-1).UtcDateTime,1):new UnknownStatus(),now.AddMinutes(-1).UtcDateTime,now.AddHours(1).UtcDateTime,null);
                    byte[] der=new OCSPRespGenerator().Generate(0,generator.Generate(signer,[bcRoot],now.UtcDateTime)).GetEncoded();string path=Path.Combine(artifacts,$"abi-{state}.ocsp.der");File.WriteAllBytes(path,der);void* response;
                    fixed(byte* d=der){byte* cursor=d;response=OpenSslAbi.d2i_OCSP_RESPONSE(null,&cursor,der.Length);Check(response!=null&&cursor==d+der.Length);}
                    void* basic=null,id=null;
                    try
                    {
                        Check(OpenSslAbi.OCSP_response_status(response)==0);basic=OpenSslAbi.OCSP_response_get1_basic(response);Check(basic!=null);id=OpenSslAbi.OCSP_cert_to_id(OpenSslAbi.EVP_sha1(),cert,anchor);Check(id!=null);
                        int status=-1,reason=-1;void* rev=null,thisUpdate=null,nextUpdate=null;Check(OpenSslAbi.OCSP_resp_find_status(basic,id,&status,&reason,&rev,&thisUpdate,&nextUpdate)==1);
                        string Time(void* value)=>value==null?"-":Encoding.ASCII.GetString(new ReadOnlySpan<byte>(OpenSslAbi.ASN1_STRING_get0_data(value),OpenSslAbi.ASN1_STRING_length(value)));
                        string result=$"{OpenSslAbi.OCSP_basic_verify(basic,chain,store,0)} {status} {Time(thisUpdate)} {Time(nextUpdate)}";Check(result==oracle(["ocsp",leafPath,rootPath,path]),"Native OCSP validation differs: "+result);Check(status==state);
                    }
                    finally{OpenSslAbi.OCSP_CERTID_free(id);OpenSslAbi.OCSP_BASICRESP_free(basic);OpenSslAbi.OCSP_RESPONSE_free(response);}
                }
                // Change only the signature in a valid ASN.1 OCSP response.
                var unsigned=new BasicOcspRespGenerator(new RespID(bcRoot.SubjectDN));unsigned.AddResponse(new CertificateID(CertificateID.DigestSha1,bcRoot,bcLeaf.SerialNumber),CertificateStatus.Good);var signed=unsigned.Generate(signer,null,now.UtcDateTime);byte[] raw=signed.GetEncoded();raw[^1]^=1;
                var corrupt=new OCSPRespGenerator().Generate(0,new BasicOcspResp(Org.BouncyCastle.Asn1.Ocsp.BasicOcspResponse.GetInstance(raw))).GetEncoded();void* badResponse;
                fixed(byte* d=corrupt){byte* cursor=d;badResponse=OpenSslAbi.d2i_OCSP_RESPONSE(null,&cursor,corrupt.Length);Check(badResponse!=null);}
                try{void* basic=OpenSslAbi.OCSP_response_get1_basic(badResponse);try{Check(OpenSslAbi.OCSP_basic_verify(basic,chain,store,0)==0);}finally{OpenSslAbi.OCSP_BASICRESP_free(basic);}}finally{OpenSslAbi.OCSP_RESPONSE_free(badResponse);}
            }
            finally{OpenSslAbi.OPENSSL_sk_free(chain);OpenSslAbi.X509_STORE_free(store);OpenSslAbi.X509_free(cert);OpenSslAbi.X509_free(anchor);}
            Check(OpenSslAbi.LiveContexts==before,"OCSP operations leaked references.");
        });
        AuditChecks.Run(test,oracle,artifacts,rootKey,root,leaf,now);
        RestrictedRsaChecks.Run(test,oracle,artifacts);
    }
    private static class VerifyCallback
    {
        public static int Calls;
        public static int IgnoreTime(int ok,void* ctx)
        {try{Calls++;int error=OpenSslAbi.X509_STORE_CTX_get_error(ctx);return error is 9 or 10?1:ok;}catch{return 0;}}
    }
}
