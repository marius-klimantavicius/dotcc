using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1.Nist;
using O=Managed.S2nTls.Crypto.OpenSslAbi;

internal static unsafe class EcPublicChecks
{
    static readonly StringBuilder Transcript=new();
    static ulong Seed;
    static List<ulong> Drain(){List<ulong> result=[];ulong e;while((e=O.ERR_get_error())!=0)result.Add(e);return result;}
    static void Start(bool seeded){Drain();if(seeded){ulong length=0;O.EVP_PKEY_decrypt(null,null,&length,null,0);O.EVP_PKEY_decrypt(null,null,&length,null,0);}}
    static void Errors(string label,bool seeded=false)
    {
        var errors=Drain();Console.Error.WriteLine(label+"|"+string.Join(',',errors.Select(e=>e.ToString("X"))));
        bool preserved=!seeded||errors.Count>=2&&errors[0]==Seed&&errors[1]==Seed;
        Transcript.Append($"|err:{(errors.Count>(seeded&&preserved?2:0)?1:0)}|seed:{(preserved?1:0)}");
    }
    static void* Read(string directory,int nid,int which)
    {
        string pem=File.ReadAllText(Path.Combine(directory,$"ec-{nid}-{which}.pem"));var fields=PemEncoding.Find(pem);
        byte[] der=Convert.FromBase64String(pem[fields.Base64Data]);fixed(byte* source=der){byte* cursor=source;void* key=O.d2i_AutoPrivateKey(null,&cursor,der.Length);if(key==null||cursor!=source+der.Length)throw new Exception("EC fixture import failed");return key;}
    }
    static void* Generate(int nid,bool parametersOnly=false)
    {
        void* context=O.EVP_PKEY_CTX_new_id(408,null),parameters=null,key=null;
        if(context==null||O.EVP_PKEY_paramgen_init(context)!=1||O.EVP_PKEY_CTX_ctrl(context,408,-1,0x1001,nid,null)!=1||O.EVP_PKEY_paramgen(context,&parameters)!=1)throw new Exception("EC parameters failed");
        O.EVP_PKEY_CTX_free(context);if(parametersOnly)return parameters;
        context=O.EVP_PKEY_CTX_new(parameters,null);
        if(context==null||O.EVP_PKEY_keygen_init(context)!=1||O.EVP_PKEY_keygen(context,&key)!=1)throw new Exception("EC generation failed");
        O.EVP_PKEY_CTX_free(context);O.EVP_PKEY_free(parameters);return key;
    }
    static byte[] Point(void* key)
    {
        byte* output=null;ulong length=O.EVP_PKEY_get1_tls_encodedpoint(key,&output);
        if(length==0||output==null)throw new Exception("EC export failed");
        try{return new ReadOnlySpan<byte>(output,(int)length).ToArray();}finally{NativeMemory.Free(output);}
    }
    static void* Public(int nid,byte[] point)
    {void* key=Generate(nid,true);fixed(byte* input=point)if(O.EVP_PKEY_set1_tls_encodedpoint(key,input,(ulong)point.Length)!=1)throw new Exception("EC public import failed");return key;}
    static int Derive(void* local,void* peer,byte[] output,ref ulong length,int offset=0,bool query=false)
    {
        void* context=O.EVP_PKEY_CTX_new(local,null);if(context==null)return -9;
        int result=O.EVP_PKEY_derive_init(context);
        if(result==1)result=O.EVP_PKEY_derive_set_peer(context,peer);
        if(result==1)fixed(byte* target=output)fixed(ulong* size=&length)result=O.EVP_PKEY_derive(context,query?null:target+offset,size);
        O.EVP_PKEY_CTX_free(context);return result;
    }
    static void Signature(void* key,void* original,void* replacement,string label)
    {
        byte[] digest=Enumerable.Range(0,32).Select(i=>(byte)(i*7+3)).ToArray(),signature=new byte[160];ulong length=160;
        void* context=O.EVP_PKEY_CTX_new(key,null);Start(false);int result=O.EVP_PKEY_sign_init(context);
        if(result==1)fixed(byte* hash=digest)fixed(byte* target=signature)result=O.EVP_PKEY_sign(context,target,&length,hash,32);
        Transcript.Append($"sign:{label}|ret:{result}");Errors(label);O.EVP_PKEY_CTX_free(context);
        foreach((nint target,int index) in new[]{((nint)original,0),((nint)key,1),((nint)replacement,2)})
        {
            context=O.EVP_PKEY_CTX_new((void*)target,null);Start(false);int verified=O.EVP_PKEY_verify_init(context);
            if(verified==1){verified=-9;if(result==1)fixed(byte* hash=digest)fixed(byte* sig=signature)verified=O.EVP_PKEY_verify(context,sig,length,hash,32);}
            Transcript.Append($"|verify{index}:{verified}");Errors(label);O.EVP_PKEY_CTX_free(context);
        }
        Transcript.AppendLine();
    }
    static void Case(string directory,int nid,bool generated,int variant,bool seeded)
    {
        void* key=generated?Generate(nid):Read(directory,nid,1),other=Read(directory,nid,2);
        byte[] old=Point(key),replacement=Point(other);int width=(replacement.Length-1)/2;byte[] input=new byte[200];replacement.CopyTo(input,0);int count=replacement.Length;
        string[] names=["uncompressed","compressed","hybrid","infinity","empty","tag1","tag5","tag8","tag255","short","long","compressed-short","compressed-long","hybrid-parity","offcurve","x-field","y-field","compressed-field","compressed-x0","compressed-x1","compressed-x2","compressed-x3"];
        if(variant is 1 or 11 or 12||variant>=17){input[0]=(byte)(2|(replacement[^1]&1));count=width+1;}
        if(variant is 2 or 13)input[0]=(byte)(6|(replacement[^1]&1));
        if(variant==3){input[0]=0;count=1;}if(variant==4)count=0;
        if(variant==5)input[0]=1;if(variant==6)input[0]=5;if(variant==7)input[0]=8;if(variant==8)input[0]=255;
        if(variant is 9 or 11)count--;if(variant is 10 or 12)input[count++]=0;if(variant==13)input[0]^=1;
        if(variant==14){input.AsSpan(0,replacement.Length).Clear();input[0]=4;}
        if(variant is >=15 and <=17)
        {
            byte[] field=NistNamedCurves.GetByName(nid==415?"P-256":nid==715?"P-384":"P-521").Curve.Field.Characteristic.ToByteArrayUnsigned();
            field.CopyTo(input,1+(variant==16?width:0));
        }
        if(variant>=18){input.AsSpan(1,width).Clear();input[width]=(byte)(variant-18);input[0]=2;}
        string label=$"{nid}:{(generated?1:0)}:{names[variant]}:{(seeded?1:0)}";void* original=Public(nid,old);
        byte[] before=new byte[80],after=new byte[80];ulong beforeLength=80,afterLength=80;
        // Keep a failed baseline operation visible in the transcript; the
        // original provider's decoded-key derive role is under comparison.
        int baseline=Derive(key,other,before,ref beforeLength);
        Start(seeded);int result;fixed(byte* source=input)result=O.EVP_PKEY_set1_tls_encodedpoint(key,source,(ulong)count);
        Transcript.Append($"set:{label}|ret:{result}");Errors(label,seeded);
        byte* point=(byte*)1;Start(false);ulong length=O.EVP_PKEY_get1_tls_encodedpoint(key,&point);
        bool wrote=point!=(byte*)1;var exported=wrote?new ReadOnlySpan<byte>(point,(int)length):ReadOnlySpan<byte>.Empty;
        Transcript.Append($"|len:{length}|wrote:{(wrote?1:0)}|old:{(wrote&&exported.SequenceEqual(old)?1:0)}|replacement:{(wrote&&exported.SequenceEqual(replacement)?1:0)}|infinity:{(wrote&&length==1&&point[0]==0?1:0)}");Errors(label);Transcript.AppendLine();if(wrote)NativeMemory.Free(point);
        void* view=O.EVP_PKEY_get1_EC_KEY(key);Start(false);int valid=O.EC_KEY_check_key(view);
        Transcript.Append($"check:{label}|ret:{valid}");Errors(label);Transcript.AppendLine();O.EC_KEY_free(view);
        Start(false);int derived=Derive(key,other,after,ref afterLength);
        Transcript.Append($"private:{label}|ret:{derived}|same:{(derived==1&&baseline==1&&before.AsSpan(0,(int)beforeLength).SequenceEqual(after.AsSpan(0,(int)afterLength))?1:0)}");Errors(label);Transcript.AppendLine();
        after.AsSpan().Fill(0xA5);afterLength=(ulong)width;Start(false);derived=Derive(other,key,after,ref afterLength,1);
        Transcript.Append($"peer:{label}|ret:{derived}|len:{afterLength}|guard:{(after[0]==0xA5&&after[width+1]==0xA5?1:0)}");Errors(label);Transcript.AppendLine();
        if(variant<=3&&!seeded)Signature(key,original,other,label);
        O.EVP_PKEY_free(original);O.EVP_PKEY_free(key);O.EVP_PKEY_free(other);
    }
    static void Outputs(string directory,int nid,bool generated)
    {
        void* local=generated?Generate(nid):Read(directory,nid,1),peer=Read(directory,nid,2);
        byte[] expected=new byte[80];ulong size=80;int baseline=Derive(local,peer,expected,ref size);
        int width=nid==415?32:nid==715?48:66;
        foreach(int capacity in new[]{0,1,width-1,width,width+1})foreach(bool query in new[]{false,true})
        {
            byte[] output=Enumerable.Repeat((byte)0xA5,84).ToArray();ulong length=(ulong)capacity;Start(true);
            int result=Derive(local,peer,output,ref length,1,query);
            bool same=result==1&&baseline==1&&!query&&length<=size&&output.AsSpan(1,(int)length).SequenceEqual(expected.AsSpan(0,(int)length));
            Transcript.Append($"capacity:{nid}:{(generated?1:0)}:{capacity}:{(query?1:0)}|ret:{result}|len:{length}|same:{(same?1:0)}|untouched:{(output.All(x=>x==0xA5)?1:0)}|guard:{(output[0]==0xA5&&output[capacity+1]==0xA5?1:0)}");Errors("capacity",true);Transcript.AppendLine();
        }
        byte[] replacement=Point(peer);void* retained=O.EVP_PKEY_CTX_new(peer,null);
        if(retained==null||O.EVP_PKEY_derive_init(retained)!=1||O.EVP_PKEY_derive_set_peer(retained,local)!=1)
        {if(retained!=null)O.EVP_PKEY_CTX_free(retained);retained=null;}
        void* view=O.EVP_PKEY_get1_EC_KEY(local);fixed(byte* source=replacement)if(O.EVP_PKEY_set1_tls_encodedpoint(local,source,(ulong)replacement.Length)!=1)throw new Exception("Retained mutation failed");
        Start(false);int valid=O.EC_KEY_check_key(view);Transcript.Append($"retained-view:{nid}:{(generated?1:0)}|check:{valid}");Errors("retained-view");Transcript.AppendLine();
        O.EC_KEY_free(view);O.EVP_PKEY_free(local);expected.AsSpan().Fill(0xA5);size=(ulong)width;Start(false);int derived;
        fixed(byte* target=expected)derived=retained==null?0:O.EVP_PKEY_derive(retained,target+1,&size);
        Transcript.Append($"retained-peer:{nid}:{(generated?1:0)}|ret:{derived}|len:{size}|guard:{(expected[0]==0xA5&&expected[(int)size+1]==0xA5?1:0)}");Errors("retained-peer");Transcript.AppendLine();
        if(retained!=null)O.EVP_PKEY_CTX_free(retained);O.EVP_PKEY_free(peer);
    }
    static void* XGenerate()
    {
        void* context=O.EVP_PKEY_CTX_new_id(1034,null),key=null;
        if(context==null||O.EVP_PKEY_keygen_init(context)!=1||O.EVP_PKEY_keygen(context,&key)!=1)throw new Exception("X25519 generation failed");
        O.EVP_PKEY_CTX_free(context);return key;
    }
    static void XContract()
    {
        foreach(int variant in Enumerable.Range(0,5))foreach(bool seeded in new[]{false,true})
        {
            void* key=XGenerate(),peer=XGenerate();byte[] old=Point(key),replacement=Point(peer),input=new byte[33];replacement.CopyTo(input,0);
            int count=variant==0?32:variant==1?0:variant==2?31:variant==3?33:32;if(variant==4)input.AsSpan().Clear();
            void* retained=O.EVP_PKEY_CTX_new(key,null);if(retained==null||O.EVP_PKEY_derive_init(retained)!=1||O.EVP_PKEY_derive_set_peer(retained,peer)!=1)throw new Exception("X25519 retained setup failed");
            Start(seeded);int result;fixed(byte* source=input)result=O.EVP_PKEY_set1_tls_encodedpoint(key,source,(ulong)count);
            Transcript.Append($"x-set:{variant}:{(seeded?1:0)}|ret:{result}");Errors("x-set",seeded);byte[] point=Point(key);
            Transcript.AppendLine($"|len:{point.Length}|old:{(point.SequenceEqual(old)?1:0)}|replacement:{(point.AsSpan().SequenceEqual(input.AsSpan(0,32))?1:0)}");
            foreach(bool query in new[]{false,true})
            {
                byte[] output=Enumerable.Repeat((byte)0xA5,40).ToArray();ulong length=37;Start(true);
                fixed(byte* target=output)result=O.EVP_PKEY_derive(retained,query?null:target+1,&length);
                Transcript.Append($"x-own:{variant}:{(seeded?1:0)}:{(query?1:0)}|ret:{result}|len:{length}|untouched:{(output.All(x=>x==0xA5)?1:0)}|guard:{(output[0]==0xA5&&output[33]==0xA5?1:0)}");Errors("x-own",true);Transcript.AppendLine();
            }
            byte[] peerOutput=Enumerable.Repeat((byte)0xA5,40).ToArray();ulong peerLength=37;Start(true);result=Derive(peer,key,peerOutput,ref peerLength,1);
            Transcript.Append($"x-peer:{variant}:{(seeded?1:0)}|ret:{result}|len:{peerLength}|zero:{(peerOutput.AsSpan(1,32).IndexOfAnyExcept((byte)0)<0?1:0)}|untouched:{(peerOutput.AsSpan(1,32).IndexOfAnyExcept((byte)0xA5)<0?1:0)}|guard:{(peerOutput[0]==0xA5&&peerOutput[33]==0xA5?1:0)}");Errors("x-peer",true);Transcript.AppendLine();
            O.EVP_PKEY_CTX_free(retained);O.EVP_PKEY_free(key);O.EVP_PKEY_free(peer);
        }
    }
    static void EmptyContract(string directory)
    {
        void* fixture=Read(directory,415,2);byte[] valid=Point(fixture);
        foreach(int kind in Enumerable.Range(0,5))foreach(bool malformed in new[]{false,true})
        {
            void* key=kind==1?Generate(415,true):O.EVP_PKEY_new();
            if(kind!=1&&kind!=4&&O.EVP_PKEY_set_type(key,kind==0?408:kind==2?1034:6)!=1)throw new Exception("Empty key setup failed");
            string label=$"{kind}:{(malformed?1:0)}";byte* point=(byte*)1;Start(true);ulong length=O.EVP_PKEY_get1_tls_encodedpoint(key,&point);
            Transcript.Append($"empty-get:{label}|len:{length}|wrote:{(point!=(byte*)1?1:0)}");Errors("empty-get",true);Transcript.AppendLine();if(point!=(byte*)1)NativeMemory.Free(point);
            byte[] input=malformed?[8]:valid;Start(true);int result;fixed(byte* source=input)result=O.EVP_PKEY_set1_tls_encodedpoint(key,source,(ulong)(malformed?1:kind==2?32:65));
            Transcript.Append($"empty-set:{label}|ret:{result}");Errors("empty-set",true);point=(byte*)1;length=O.EVP_PKEY_get1_tls_encodedpoint(key,&point);
            Transcript.Append($"|len:{length}|wrote:{(point!=(byte*)1?1:0)}|infinity:{(length==1&&point!=(byte*)1&&point[0]==0?1:0)}");Errors("empty-after");Transcript.AppendLine();if(point!=(byte*)1)NativeMemory.Free(point);O.EVP_PKEY_free(key);Drain();
        }
        O.EVP_PKEY_free(fixture);
    }
    static void ViewContract(string directory)
    {
        foreach(int nid in new[]{415,715,716})
        {
            void* key=Read(directory,nid,1);Start(true);void* first=O.EVP_PKEY_get1_EC_KEY(key),second=O.EVP_PKEY_get1_EC_KEY(key);
            if(first==null||second==null)throw new Exception("EC views failed");
            Transcript.Append($"views:{nid}|alias:{(first==second?1:0)}|group-alias:{(O.EC_KEY_get0_group(first)==O.EC_KEY_get0_group(second)?1:0)}");Errors("views",true);Transcript.AppendLine();
            O.EC_KEY_free(first);O.EC_KEY_free(second);Start(false);void* again=O.EVP_PKEY_get1_EC_KEY(key);if(again==null)throw new Exception("EC reget failed");
            Transcript.Append($"reget:{nid}|alias:{(again==first?1:0)}|curve:{O.EC_GROUP_get_curve_name(O.EC_KEY_get0_group(again))}");Errors("reget");Transcript.AppendLine();
            O.EVP_PKEY_free(key);Start(true);int valid=O.EC_KEY_check_key(again);Transcript.Append($"view-retained:{nid}|ret:{valid}");Errors("view-retained",true);Transcript.AppendLine();O.EC_KEY_free(again);
        }
        foreach(bool parameters in new[]{false,true})
        {
            void* key=parameters?Generate(415,true):O.EVP_PKEY_new();if(!parameters&&O.EVP_PKEY_set_type(key,408)!=1)throw new Exception("EC empty setup failed");
            Start(true);void* view=O.EVP_PKEY_get1_EC_KEY(key);Transcript.Append($"view-empty:{(parameters?1:0)}|present:{(view!=null?1:0)}");Errors("view-empty",true);Transcript.AppendLine();O.EC_KEY_free(view);O.EVP_PKEY_free(key);
        }
    }
    public static string BuildTranscript(string directory)
    {
        Transcript.Clear();int baseline=O.LiveContexts;ulong length=0;O.EVP_PKEY_decrypt(null,null,&length,null,0);Seed=Drain().Single();
        foreach(int nid in new[]{415,715,716})foreach(bool generated in new[]{false,true})foreach(int variant in Enumerable.Range(0,22))foreach(bool seeded in new[]{false,true})Case(directory,nid,generated,variant,seeded);
        foreach(int nid in new[]{415,715,716})foreach(bool generated in new[]{false,true})Outputs(directory,nid,generated);
        XContract();
        EmptyContract(directory);
        ViewContract(directory);
        if(O.LiveContexts!=baseline)throw new Exception("EC public-state group leaked contexts");if(Drain().Count!=0)throw new Exception("EC group left errors");return Transcript.ToString();
    }
    public static void Run(Action<string,Action> test,Func<string[],string> oracle,string artifacts,string nativeVersion)
    {
        test("abi/pkey/ec-public-state-and-private-roles",()=>
        {
            string fixtures=Path.Combine(artifacts,"ec-public-fixtures");Directory.CreateDirectory(fixtures);
            foreach(int nid in new[]{415,715,716})foreach(int which in new[]{1,2})
            {using var key=ECDsa.Create(Managed.S2nTls.Crypto.EcKeyExchange.Curve(nid==415?"P-256":nid==715?"P-384":"P-521"));File.WriteAllText(Path.Combine(fixtures,$"ec-{nid}-{which}.pem"),key.ExportPkcs8PrivateKeyPem());}
            string actual=BuildTranscript(fixtures).Trim(),expected=oracle(["ec-public-contract",fixtures,nativeVersion]);
            File.WriteAllText(Path.Combine(artifacts,"ec-public-managed.txt"),actual+"\n");File.WriteAllText(Path.Combine(artifacts,"ec-public-native.txt"),expected+"\n");
            if(actual!=expected)
            {var a=actual.Split('\n');var b=expected.Split('\n');int row=Enumerable.Range(0,Math.Min(a.Length,b.Length)).FirstOrDefault(i=>a[i]!=b[i],-1);throw new Exception($"EC public-state native differential differs at row {row}; rows {a.Length}/{b.Length}");}
            Console.WriteLine($"EC public-state native differential vectors: {actual.Split('\n').Length}");
        });
    }
}
