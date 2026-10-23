using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Math;
using O = Managed.S2nTls.Crypto.OpenSslAbi;

internal static unsafe class RsaPrivateChecks
{
    static readonly StringBuilder Transcript = new();
    static int Width;
    static ulong Seed;
    static List<ulong> Drain()
    {
        List<ulong> result = [];
        ulong error;
        while ((error = O.ERR_get_error()) != 0) result.Add(error);
        return result;
    }
    static void Check(void* ctx,string label,byte[] input,int count,int capacity,bool query=false,bool seeded=false,bool nullInput=false)
    {
        Drain();
        ulong length = (ulong)capacity;
        if (seeded) { O.EVP_PKEY_decrypt(null,null,&length,null,0); length=(ulong)capacity; }
        byte[] output = Enumerable.Repeat((byte)0xA5,Width+3).ToArray();
        int result;
        fixed(byte* source=input) fixed(byte* target=output)
            result=O.EVP_PKEY_decrypt(ctx,query?null:target+1,&length,nullInput?null:source,count<0?ulong.MaxValue:(ulong)count);
        var errors=Drain();
        bool preserved=!seeded || errors.Count>0&&errors[0]==Seed;
        bool newError=errors.Count>(seeded?1:0);
        Transcript.AppendLine($"{label}|{(seeded?1:0)}|{result}|{length}|{(newError?1:0)}|{(preserved?1:0)}|{Convert.ToHexString(output)}");
    }
    static void Both(void* ctx,string label,byte[] input,int count,int capacity,bool query=false,bool nullInput=false)
    { Check(ctx,label,input,count,capacity,query,false,nullInput);Check(ctx,label,input,count,capacity,query,true,nullInput); }
    static void Init(void* ctx,string label)
    {
        Drain();int result=O.EVP_PKEY_decrypt_init(ctx);var errors=Drain();
        Transcript.AppendLine($"init:{label}|{result}|{(errors.Count>0?1:0)}");
    }
    static byte[] Cipher(byte[] encoded,BigInteger modulus,BigInteger exponent)
    {
        byte[] raw=new BigInteger(1,encoded).ModPow(exponent,modulus).ToByteArrayUnsigned();
        byte[] result=new byte[Width];raw.CopyTo(result,Width-raw.Length);return result;
    }
    public static void Run(Action<string,Action> test,Func<string[],string> oracle,string artifacts,string nativeVersion)
    {
        test("abi/pkey/rsa-native-input-limits-output-contract", () =>
        {
            using RSA fixture = RSA.Create(2048);
            string path = Path.Combine(artifacts,"rsa-private-contract.pem");
            File.WriteAllText(path,fixture.ExportPkcs8PrivateKeyPem());
            string actual = BuildTranscript(path).Trim();
            string expected = oracle(["rsa-private-contract",path,nativeVersion]);
            if (actual != expected)
            {
                string[] managed = actual.Split('\n'), native = expected.Split('\n');
                int index = Enumerable.Range(0,Math.Min(managed.Length,native.Length)).FirstOrDefault(i => managed[i] != native[i], -1);
                throw new Exception($"Private RSA native differential differs at row {index}; managed/native rows {managed.Length}/{native.Length}");
            }
            Console.WriteLine($"RSA private native differential vectors: {actual.Split('\n').Length}");
        });
    }
    private static string BuildTranscript(string fixturePath)
    {
        Transcript.Clear();
        int baseline=O.LiveContexts;
        using RSA fixture=RSA.Create();fixture.ImportFromPem(File.ReadAllText(fixturePath));
        byte[] der=fixture.ExportPkcs8PrivateKey();void* key;
        fixed(byte* source=der) { byte* cursor=source;key=O.d2i_AutoPrivateKey(null,&cursor,der.Length);if(key==null||cursor!=source+der.Length)throw new Exception("Fixture import failed"); }
        Width=O.EVP_PKEY_size(key);var parameters=fixture.ExportParameters(false);
        var modulus=new BigInteger(1,parameters.Modulus!);var exponent=new BigInteger(1,parameters.Exponent!);
        ulong seedLength=0;O.EVP_PKEY_decrypt(null,null,&seedLength,null,0);Seed=Drain().Single();
        void* ctx=O.EVP_PKEY_CTX_new(key,null);void* retained=O.EVP_PKEY_CTX_new(key,null);
        if(ctx==null||retained==null)throw new Exception("Context allocation failed");
        byte[] zeros=new byte[Width+1];
        Both(ctx,"before-init",zeros,Width,Width);
        Both(ctx,"before-init-query",zeros,Width,0,true);
        if(O.EVP_PKEY_encrypt_init(ctx)!=1)throw new Exception("Encrypt init failed");
        Both(ctx,"encrypt-operation",zeros,Width,Width);
        Init(ctx,"rsa");Init(retained,"retained");O.EVP_PKEY_free(key);
        foreach(int padding in new[]{3,1})
        {
            if(O.RSA_pkey_ctx_ctrl(ctx,-1,0x1001,padding,null)!=1)throw new Exception("Padding control failed");
            var values=new[]{BigInteger.Zero,BigInteger.One,modulus.Subtract(BigInteger.One),modulus,modulus.Add(BigInteger.One)};
            for(int kind=0;kind<values.Length;kind++) foreach(int count in new[]{0,1,Width-1,Width,Width+1})
            {
                byte[] raw=values[kind].ToByteArrayUnsigned(),input=new byte[count];
                int copy=Math.Min(count,raw.Length);raw.AsSpan(raw.Length-copy,copy).CopyTo(input.AsSpan(count-copy));
                foreach(int capacity in new[]{0,1,Width-1,Width,Width+1})
                    Both(ctx,$"p{padding}:v{kind}:n{count}:cap{capacity}",input,count,capacity);
                Both(ctx,$"p{padding}:v{kind}:n{count}:query",input,count,7,true);
            }
            Both(ctx,$"p{padding}:null-zero",zeros,0,Width,false,true);
            Both(ctx,$"p{padding}:query-invalid-input",zeros,-1,0,true,true);
            Both(ctx,$"p{padding}:small-invalid-input",zeros,-1,Width-1,false,true);
        }
        // Deterministic public exponentiation creates exact valid/malformed EMs.
        // The production private operation remains BC's RsaBlindedEngine.
        for(int mutation=-1;mutation<Width;mutation++)
        {
            byte[] encoded=Enumerable.Repeat((byte)0x7F,Width).ToArray();encoded[0]=0;encoded[1]=2;
            int delimiter=Width-49;encoded[delimiter]=0;for(int i=0;i<48;i++)encoded[delimiter+1+i]=(byte)i;
            if(mutation>=0){if(mutation>1&&mutation<delimiter)encoded[mutation]=0;else encoded[mutation]++;}
            byte[] cipher=Cipher(encoded,modulus,exponent);
            Both(ctx,"pkcs1-mutation-"+mutation,cipher,Width,Width);
        }
        foreach(int delimiter in new[]{2,3,8,9,10,11,Width-2,Width-1,Width})
        {
            byte[] encoded=Enumerable.Repeat((byte)0x7F,Width).ToArray();encoded[0]=0;encoded[1]=2;
            if(delimiter<Width)encoded[delimiter]=0;
            byte[] cipher=Cipher(encoded,modulus,exponent);
            Both(ctx,"pkcs1-delimiter-"+delimiter,cipher,Width,Width);
        }
        O.EVP_PKEY_CTX_free(ctx);
        Both(retained,"other-context-survives",zeros,Width,Width);
        if(O.EVP_PKEY_sign_init(retained)!=1)throw new Exception("Sign init failed");
        Both(retained,"sign-operation",zeros,Width,Width);
        Init(retained,"reinit");
        if(O.RSA_pkey_ctx_ctrl(retained,-1,0x1001,3,null)!=1)throw new Exception("Retained padding failed");
        Both(retained,"reinit-raw",zeros,Width,Width);O.EVP_PKEY_CTX_free(retained);
        foreach(int type in new[]{6,408,1034})
        {
            void* missing=O.EVP_PKEY_CTX_new_id(type,null);if(missing==null)throw new Exception("ID context failed");
            Init(missing,"id-"+type);
            Both(missing,"id-"+type,zeros,Width,Width);
            Both(missing,"id-query-"+type,zeros,Width,0,true);O.EVP_PKEY_CTX_free(missing);
        }
        Init(null,"null");Both(null,"null-context",zeros,Width,Width);
        if(O.LiveContexts!=baseline)throw new Exception("Private RSA context ownership leaked");
        if(Drain().Count!=0)throw new Exception("Unconsumed private RSA errors");
        return Transcript.ToString();
    }
}
