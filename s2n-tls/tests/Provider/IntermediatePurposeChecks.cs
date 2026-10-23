using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Managed.S2nTls.Crypto;

internal static unsafe class IntermediatePurposeChecks
{
    private static void Check(bool value,string message){if(!value)throw new Exception(message);}
    private static List<byte[]> ReadCertificates(string path)
    {
        var result=new List<byte[]>();ReadOnlySpan<char> text=File.ReadAllText(path);
        while(PemEncoding.TryFind(text,out var fields))
        {if(text[fields.Label].SequenceEqual("CERTIFICATE"))result.Add(Convert.FromBase64String(text[fields.Base64Data].ToString()));text=text[fields.Location.End..];}
        return result;
    }
    private static void* Decode(byte[] der){fixed(byte* p=der){byte* cursor=p;void* cert=OpenSslAbi.d2i_X509(null,&cursor,der.Length);Check(cert!=null&&cursor==p+der.Length,"Certificate decode");return cert;}}
    internal static void Run(Action<string,Action> test,Func<string[],string> oracle)
    {
        test("audit/x509-original-intent-critical-intermediate-native",()=>
        {
            int before=OpenSslAbi.LiveContexts;var fixtures=Path.Combine(Directory.GetCurrentDirectory(),"s2n-tls/ref/s2n-tls/tests/pems/intent/cert_chains");int count=0;
            foreach(string directory in Directory.EnumerateDirectories(fixtures).Order())
            {
                string chainPath=Path.Combine(directory,"cert-chain.pem"),rootPath=Path.Combine(directory,"root-cert.pem");if(!File.Exists(chainPath)||!File.Exists(rootPath))continue;
                var certs=ReadCertificates(chainPath).Select(x=>(nint)Decode(x)).ToList();void* root=Decode(ReadCertificates(rootPath)[0]),store=OpenSslAbi.X509_STORE_new(),context=OpenSslAbi.X509_STORE_CTX_new(),wire=OpenSslAbi.OPENSSL_sk_new_null();
                try
                {
                    Check(certs.Count>0,"Fixture has no leaf");foreach(nint cert in certs.Skip(1))Check(OpenSslAbi.OPENSSL_sk_push(wire,(void*)cert)>0,"Push intermediate");
                    Check(OpenSslAbi.X509_STORE_add_cert(store,root)==1&&OpenSslAbi.X509_STORE_CTX_init(context,store,(void*)certs[0],wire)==1,"Context setup");OpenSslAbi.X509_STORE_CTX_set_flags(context,0x80000);int ok=OpenSslAbi.X509_verify_cert(context),error=OpenSslAbi.X509_STORE_CTX_get_error(context);string native=oracle(["x509-chain",chainPath,rootPath]);Check($"{ok} {error}"==native,Path.GetFileName(directory)+$": managed {ok} {error}, native {native}");count++;
                }
                finally{OpenSslAbi.X509_STORE_CTX_free(context);OpenSslAbi.X509_STORE_free(store);OpenSslAbi.OPENSSL_sk_free(wire);OpenSslAbi.X509_free(root);foreach(nint cert in certs)OpenSslAbi.X509_free((void*)cert);}
            }
            Check(count>=20,"Original intent fixtures were not enumerated");Check(OpenSslAbi.LiveContexts==before,"Intermediate validation leaked handles");
        });
    }
}
