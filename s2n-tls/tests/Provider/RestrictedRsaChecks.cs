using System.Security.Cryptography;
using Managed.S2nTls.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

internal static unsafe class RestrictedRsaChecks
{
    private static void Check(bool value,string message){if(!value)throw new Exception(message);}
    internal static void Run(Action<string,Action> test,Func<string[],string> oracle,string artifacts)
    {
        test("audit/rsa-pss-restricted-pkcs8-spki-native",()=>
        {
            int before=OpenSslAbi.LiveContexts;using var rsa=RSA.Create(2048);
            var original=PrivateKeyInfo.GetInstance(rsa.ExportPkcs8PrivateKey());var publicOriginal=SubjectPublicKeyInfo.GetInstance(rsa.ExportSubjectPublicKeyInfo());
            var hash=new AlgorithmIdentifier(new DerObjectIdentifier("2.16.840.1.101.3.4.2.1"),DerNull.Instance);var mgf=new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1,hash);
            foreach(int saltMinimum in new[]{32,33})
            {
                var algorithm=new AlgorithmIdentifier(PkcsObjectIdentifiers.IdRsassaPss,new RsassaPssParameters(hash,mgf,DerInteger.ValueOf(saltMinimum),DerInteger.ValueOf(1)));
                byte[] der=new PrivateKeyInfo(algorithm,original.ParsePrivateKey()).GetDerEncoded();string publicPath=Path.Combine(artifacts,"restricted-pss-public.pem");File.WriteAllText(publicPath,PemEncoding.WriteString("PUBLIC KEY",new SubjectPublicKeyInfo(algorithm,publicOriginal.PublicKey).GetDerEncoded()));
                void* key;fixed(byte* p=der){byte* cursor=p;key=OpenSslAbi.d2i_AutoPrivateKey(null,&cursor,der.Length);Check(key!=null&&cursor==p+der.Length,"Restricted PSS private decode");}
                Check(OpenSslAbi.EVP_PKEY_id(key)==912,"RSA-PSS key type erased");void* context=OpenSslAbi.EVP_PKEY_CTX_new(key,null);OpenSslAbi.EVP_PKEY_free(key);
                try
                {
                    Check(OpenSslAbi.EVP_PKEY_sign_init(context)==1,"PSS sign init");Check(OpenSslAbi.EVP_PKEY_CTX_ctrl(context,-1,-1,1,0,OpenSslAbi.EVP_sha256())==1,"PSS signature digest");Check(OpenSslAbi.RSA_pkey_ctx_ctrl(context,-1,0x1001,6,null)==1&&OpenSslAbi.RSA_pkey_ctx_ctrl(context,-1,0x1002,-1,null)==1,"PSS padding/salt");
                    byte[] digest=SHA256.HashData("restricted RSA-PSS"u8),signature=new byte[256];ulong length=(ulong)signature.Length;
                    fixed(byte* d=digest)fixed(byte* s=signature)
                    {
                        int signed=OpenSslAbi.EVP_PKEY_sign(context,s,&length,d,(ulong)digest.Length);
                        if(saltMinimum==33){Check(signed==0,"Minimum PSS salt restriction ignored");continue;}
                        Check(signed==1&&length==256,"Valid restricted PSS signing rejected");Check(oracle(["rsa-verify",publicPath,"sha256","pss",Convert.ToHexString(digest),Convert.ToHexString(signature)])=="1","Native restricted PSS verification");
                        Check(OpenSslAbi.EVP_PKEY_verify_init(context)==1&&OpenSslAbi.EVP_PKEY_verify(context,s,length,d,(ulong)digest.Length)==1,"Restricted PSS self verify");
                        Check(OpenSslAbi.EVP_PKEY_sign_init(context)==1&&OpenSslAbi.EVP_PKEY_CTX_ctrl(context,-1,-1,1,0,OpenSslAbi.EVP_sha384())==1,"Change PSS digest");byte[] mismatch=SHA384.HashData("restricted RSA-PSS"u8);fixed(byte* m=mismatch)Check(OpenSslAbi.EVP_PKEY_sign(context,s,&length,m,(ulong)mismatch.Length)==0,"PSS hash/MGF restriction ignored");
                    }
                }
                finally{OpenSslAbi.EVP_PKEY_CTX_free(context);}
            }
            Check(OpenSslAbi.LiveContexts==before,"Restricted PSS handles leaked");
        });
    }
}
