using System.Security.Cryptography;
using System.Text;
using Managed.S2nTls.Crypto;

internal static unsafe class AbiChecks
{
    private static void Check(bool value){if(!value)throw new Exception("Provider ABI assertion failed.");}
    private static void Same(ReadOnlySpan<byte> a,ReadOnlySpan<byte> b){Check(a.SequenceEqual(b));}
    private static byte[] Bytes(string hex)=>Convert.FromHexString(hex);
    private static string Hex(ReadOnlySpan<byte> bytes)=>Convert.ToHexString(bytes);
    internal static void Run(Action<string,Action> test,Func<string[],string> oracle,string artifacts)
    {
        test("abi/shake256-xof-native-clone-width-final-reset",()=>
        {
            int before=OpenSslAbi.LiveContexts;byte[] input="original s2n SHAKE path"u8.ToArray();Check(OpenSslAbi.EVP_MD_size(OpenSslAbi.EVP_shake256())==32);
            byte* fixedOutput=stackalloc byte[32];
            foreach(int width in new[]{0,1,32,64,137})
            {
                void* ctx=OpenSslAbi.EVP_MD_CTX_new(),copy=OpenSslAbi.EVP_MD_CTX_new();
                try{Check(OpenSslAbi.EVP_DigestInit_ex(ctx,OpenSslAbi.EVP_shake256(),null)==1);fixed(byte* p=input){Check(OpenSslAbi.EVP_DigestUpdate(ctx,p,(ulong)input.Length)==1);}Check(OpenSslAbi.EVP_MD_CTX_copy_ex(copy,ctx)==1);byte[] output=new byte[width+2];output[0]=output[^1]=0xa5;
                    fixed(byte* p=output){Check(OpenSslAbi.EVP_DigestFinalXOF(ctx,p+1,(ulong)width)==1);Check(OpenSslAbi.EVP_DigestFinalXOF(ctx,p+1,(ulong)width)==0);}Check(output[0]==0xa5&&output[^1]==0xa5);Same(output.AsSpan(1,width),Bytes(oracle(["hash-xof",Hex(input),width.ToString()])));
                    uint size=0;Check(OpenSslAbi.EVP_DigestFinal_ex(copy,fixedOutput,&size)==1&&size==32);Same(new ReadOnlySpan<byte>(fixedOutput,32),Bytes(oracle(["hash","shake256",Hex(input)])));
                    Check(OpenSslAbi.EVP_MD_CTX_reset(ctx)==1&&OpenSslAbi.EVP_DigestInit_ex(ctx,OpenSslAbi.EVP_sha256(),null)==1);Check(OpenSslAbi.EVP_DigestFinalXOF(ctx,fixedOutput,32)==0);
                }finally{OpenSslAbi.EVP_MD_CTX_free(ctx);OpenSslAbi.EVP_MD_CTX_free(copy);}
            }
            Check(OpenSslAbi.LiveContexts==before);
        });
        test("abi/digest-copy-final-reset-error-lifetime",()=>
        {
            int before=OpenSslAbi.LiveContexts;byte[] message=Encoding.ASCII.GetBytes("state clone across boundaries");
            void* ctx=OpenSslAbi.EVP_MD_CTX_new(),copy=OpenSslAbi.EVP_MD_CTX_new();Check(ctx!=null&&copy!=null);
            try
            {
                Check(OpenSslAbi.EVP_DigestInit_ex(ctx,OpenSslAbi.EVP_md5_sha1(),null)==1);
                fixed(byte* p=message){Check(OpenSslAbi.EVP_DigestUpdate(ctx,p,7)==1);Check(OpenSslAbi.EVP_MD_CTX_copy_ex(copy,ctx)==1);Check(OpenSslAbi.EVP_DigestUpdate(ctx,p+7,(ulong)message.Length-7)==1);Check(OpenSslAbi.EVP_DigestUpdate(copy,p+7,(ulong)message.Length-7)==1);}
                byte* output=stackalloc byte[38];output[0]=output[37]=0xa5;uint size=0;
                Check(OpenSslAbi.EVP_DigestFinal_ex(ctx,output+1,&size)==1&&size==36);Same(new ReadOnlySpan<byte>(output+1,36),Bytes(oracle(["hash","md5-sha1",Hex(message)])));Check(output[0]==0xa5&&output[37]==0xa5);
                Check(OpenSslAbi.EVP_DigestFinal_ex(ctx,output+1,&size)==0&&size==0);Check(OpenSslAbi.ERR_get_error()!=0);while(OpenSslAbi.ERR_get_error()!=0){}
                Check(OpenSslAbi.EVP_DigestFinal_ex(copy,output+1,&size)==1&&size==36);Check(OpenSslAbi.EVP_MD_CTX_reset(ctx)==1);Check(OpenSslAbi.EVP_MD_CTX_md(ctx)==null);
                Check(OpenSslAbi.EVP_DigestUpdate(ctx,null,1)==0);
            }
            finally{OpenSslAbi.EVP_MD_CTX_free(ctx);OpenSslAbi.EVP_MD_CTX_free(copy);}
            Check(OpenSslAbi.LiveContexts==before);
        });
        test("abi/rc4-continuity-cbc-chaining",()=>
        {
            int before=OpenSslAbi.LiveContexts;
            foreach(var item in new (string Name,nint Descriptor,int Key,int Block,int Iv)[]{("rc4",(nint)OpenSslAbi.EVP_rc4(),16,1,0),("aes-128-cbc",(nint)OpenSslAbi.EVP_aes_128_cbc(),16,16,16),("des-ede3-cbc",(nint)OpenSslAbi.EVP_des_ede3_cbc(),24,8,8)})
            {
                byte[] key=Enumerable.Range(0,item.Key).Select(x=>(byte)(x+1)).ToArray(),iv=new byte[item.Iv],message=Enumerable.Range(0,64).Select(x=>(byte)x).ToArray(),output=new byte[64];
                void* ctx=OpenSslAbi.EVP_CIPHER_CTX_new();try
                {
                    fixed(byte* k=key)fixed(byte* v=iv)fixed(byte* input=message)fixed(byte* destination=output)
                    {
                        Check(OpenSslAbi.EVP_EncryptInit_ex(ctx,(void*)item.Descriptor,null,k,v)==1);Check(OpenSslAbi.EVP_CIPHER_CTX_set_padding(ctx,0)==1);int length=0;
                        Check(OpenSslAbi.EVP_EncryptUpdate(ctx,destination,&length,input,item.Block)==1&&length==item.Block);
                        Check(OpenSslAbi.EVP_EncryptUpdate(ctx,destination+item.Block,&length,input+item.Block,64-item.Block)==1&&length==64-item.Block);
                    }
                    Same(output,Bytes(oracle(["cipher",item.Name,Hex(key),Hex(iv),Hex(message),item.Block.ToString(),"1"])));
                }
                finally{OpenSslAbi.EVP_CIPHER_CTX_free(ctx);}
            }
            Check(OpenSslAbi.LiveContexts==before);
        });
        test("abi/aead-record-sequence-bad-tag-empty",()=>
        {
            int before=OpenSslAbi.LiveContexts;
            foreach(var item in new[]{("aes-128-gcm",(nint)OpenSslAbi.EVP_aes_128_gcm(),16),("aes-256-gcm",(nint)OpenSslAbi.EVP_aes_256_gcm(),32),("chacha20-poly1305",(nint)OpenSslAbi.EVP_chacha20_poly1305(),32)})
            foreach(int inputSize in new[]{0,87})
            {
                byte[] key=new byte[item.Item3],nonce=new byte[12],aad=[1,2,3,4],input=new byte[Math.Max(1,inputSize)],output=new byte[Math.Max(1,inputSize)],tag=new byte[16],clear=new byte[Math.Max(1,inputSize)];
                void* encrypt=OpenSslAbi.EVP_CIPHER_CTX_new(),decrypt=OpenSslAbi.EVP_CIPHER_CTX_new();
                try
                {
                    fixed(byte* k=key)fixed(byte* iv=nonce)fixed(byte* a=aad)fixed(byte* p=input)fixed(byte* c=output)fixed(byte* t=tag)fixed(byte* plain=clear)
                    {
                        int length=0;
                        Check(OpenSslAbi.EVP_EncryptInit_ex(encrypt,(void*)item.Item2,null,null,null)==1);Check(OpenSslAbi.EVP_CIPHER_CTX_ctrl(encrypt,9,12,null)==1);
                        Check(OpenSslAbi.EVP_EncryptInit_ex(encrypt,null,null,k,null)==1);Check(OpenSslAbi.EVP_EncryptInit_ex(encrypt,null,null,null,iv)==1);
                        Check(OpenSslAbi.EVP_EncryptUpdate(encrypt,null,&length,a,aad.Length)==1);Check(OpenSslAbi.EVP_EncryptUpdate(encrypt,c,&length,p,inputSize)==1&&length==inputSize);
                        Check(OpenSslAbi.EVP_EncryptFinal_ex(encrypt,c,&length)==1&&length==0);Check(OpenSslAbi.EVP_CIPHER_CTX_ctrl(encrypt,0x10,16,t)==1);
                        Same([..output.AsSpan(0,inputSize),..tag],Bytes(oracle(["aead",item.Item1,Hex(key),Hex(nonce),Hex(input.AsSpan(0,inputSize)),Hex(aad),"-"])));
                        Check(OpenSslAbi.EVP_DecryptInit_ex(decrypt,(void*)item.Item2,null,k,null)==1);
                        foreach(bool corrupt in new[]{false,true})
                        {
                            Check(OpenSslAbi.EVP_DecryptInit_ex(decrypt,null,null,null,iv)==1);if(corrupt)t[0]^=1;
                            Check(OpenSslAbi.EVP_CIPHER_CTX_ctrl(decrypt,0x11,16,t)==1);Check(OpenSslAbi.EVP_DecryptUpdate(decrypt,null,&length,a,aad.Length)==1);
                            int result=OpenSslAbi.EVP_DecryptUpdate(decrypt,plain,&length,c,inputSize);result&=OpenSslAbi.EVP_DecryptFinal_ex(decrypt,plain,&length);
                            Check(result==(corrupt?0:1));Same(clear.AsSpan(0,inputSize),new byte[inputSize]);
                        }
                    }
                }
                finally{OpenSslAbi.EVP_CIPHER_CTX_free(encrypt);OpenSslAbi.EVP_CIPHER_CTX_free(decrypt);}
            }
            Check(OpenSslAbi.LiveContexts==before);
        });
        test("abi/rsa-import-sign-encrypt-raw-unpad-boundary",()=>
        {
            using var raw=RSA.Create(1024);byte[] der=raw.ExportPkcs8PrivateKey(),hash=Digest.Hash(DigestAlgorithm.Md5Sha1,"legacy"u8),output=new byte[128];
            void* key=null,ctx=null;int before=OpenSslAbi.LiveContexts;
            try
            {
                fixed(byte* d=der){byte* cursor=d;key=OpenSslAbi.d2i_AutoPrivateKey(null,&cursor,der.Length);Check(key!=null&&cursor==d+der.Length);}
                Check(OpenSslAbi.EVP_PKEY_id(key)==6&&OpenSslAbi.EVP_PKEY_bits(key)==1024&&OpenSslAbi.EVP_PKEY_size(key)==128);
                ctx=OpenSslAbi.EVP_PKEY_CTX_new(key,null);Check(ctx!=null);OpenSslAbi.EVP_PKEY_free(key);key=null;
                Check(OpenSslAbi.EVP_PKEY_sign_init(ctx)==1);Check(OpenSslAbi.EVP_PKEY_CTX_ctrl(ctx,-1,-1,1,0,OpenSslAbi.EVP_md5_sha1())==1);
                fixed(byte* h=hash)fixed(byte* o=output){ulong length=128;Check(OpenSslAbi.EVP_PKEY_sign(ctx,o,&length,h,(ulong)hash.Length)==1&&length==128);}
                string path=Path.Combine(artifacts,"abi-rsa.pem");File.WriteAllText(path,raw.ExportPkcs8PrivateKeyPem());Same(output,Bytes(oracle(["rsa-sign",path,"md5-sha1","pkcs1",Hex(hash)])));
                byte[] encrypted=raw.Encrypt("premaster"u8,RSAEncryptionPadding.Pkcs1);Check(OpenSslAbi.EVP_PKEY_decrypt_init(ctx)==1);Check(OpenSslAbi.EVP_PKEY_CTX_ctrl(ctx,6,-1,0x1001,3,null)==1);
                fixed(byte* c=encrypted)fixed(byte* o=output){ulong length=128;Check(OpenSslAbi.EVP_PKEY_decrypt(ctx,o,&length,c,(ulong)encrypted.Length)==1&&length==128);}
                Same(output,Bytes(oracle(["rsa-raw-decrypt",path,Hex(encrypted)])));
                Check(output[0]==0&&output[1]==2&&output[118]==0);Same(output.AsSpan(119),"premaster"u8);
                // Malformed DER must preserve the input pointer and return NULL.
                byte* bad=stackalloc byte[2];bad[0]=0x30;bad[1]=0xff;byte* original=bad;Check(OpenSslAbi.d2i_AutoPrivateKey(null,&bad,2)==null&&bad==original);
            }
            finally{OpenSslAbi.EVP_PKEY_CTX_free(ctx);OpenSslAbi.EVP_PKEY_free(key);}
            Check(OpenSslAbi.LiveContexts==before);
        });
        test("abi/ecdh-actual-pkey-paramgen-keygen-derive",()=>
        {
            int before=OpenSslAbi.LiveContexts;
            foreach(int curve in new[]{415,715,716,1034})
            {
                void* generator=null,parameters=null,alice=null,bob=null,context=null;
                try
                {
                    generator=OpenSslAbi.EVP_PKEY_CTX_new_id(curve==1034?1034:408,null);Check(generator!=null);
                    if(curve!=1034)
                    {
                        Check(OpenSslAbi.EVP_PKEY_paramgen_init(generator)==1);Check(OpenSslAbi.EVP_PKEY_CTX_ctrl(generator,408,-1,0x1001,curve,null)==1);Check(OpenSslAbi.EVP_PKEY_paramgen(generator,&parameters)==1);
                        OpenSslAbi.EVP_PKEY_CTX_free(generator);generator=OpenSslAbi.EVP_PKEY_CTX_new(parameters,null);
                    }
                    Check(OpenSslAbi.EVP_PKEY_keygen_init(generator)==1);Check(OpenSslAbi.EVP_PKEY_keygen(generator,&alice)==1);Check(OpenSslAbi.EVP_PKEY_keygen(generator,&bob)==1);
                    context=OpenSslAbi.EVP_PKEY_CTX_new(alice,null);Check(OpenSslAbi.EVP_PKEY_derive_init(context)==1);Check(OpenSslAbi.EVP_PKEY_derive_set_peer(context,bob)==1);ulong length=0;Check(OpenSslAbi.EVP_PKEY_derive(context,null,&length)==1);
                    byte[] first=new byte[length],second=new byte[length];fixed(byte* a=first){Check(OpenSslAbi.EVP_PKEY_derive(context,a,&length)==1);}
                    OpenSslAbi.EVP_PKEY_CTX_free(context);context=OpenSslAbi.EVP_PKEY_CTX_new(bob,null);Check(OpenSslAbi.EVP_PKEY_derive_init(context)==1);Check(OpenSslAbi.EVP_PKEY_derive_set_peer(context,alice)==1);
                    fixed(byte* b=second){Check(OpenSslAbi.EVP_PKEY_derive(context,b,&length)==1);}Same(first,second);
                    if(curve!=1034){void* ec=OpenSslAbi.EVP_PKEY_get1_EC_KEY(alice);try{Check(OpenSslAbi.EC_KEY_check_key(ec)==1);Check(OpenSslAbi.EC_GROUP_get_curve_name(OpenSslAbi.EC_KEY_get0_group(ec))==curve);}finally{OpenSslAbi.EC_KEY_free(ec);}}
                }
                finally{OpenSslAbi.EVP_PKEY_CTX_free(context);OpenSslAbi.EVP_PKEY_CTX_free(generator);OpenSslAbi.EVP_PKEY_free(parameters);OpenSslAbi.EVP_PKEY_free(alice);OpenSslAbi.EVP_PKEY_free(bob);}
            }
            Check(OpenSslAbi.LiveContexts==before);
        });
        test("abi/bn-dh-validation-ownership-native",()=>
        {
            int before=OpenSslAbi.LiveContexts;
            foreach(var pair in new[]{("17","05"),("17","01"),("0F","02"),("1D","02"),("18","02")})
            {
                void* dh=OpenSslAbi.DH_new();try
                {
                    byte[] p=Bytes(pair.Item1),g=Bytes(pair.Item2);void* bnP,bnG;
                    fixed(byte* prime=p)fixed(byte* generator=g){bnP=OpenSslAbi.BN_bin2bn(prime,p.Length,null);bnG=OpenSslAbi.BN_bin2bn(generator,g.Length,null);}
                    Check(OpenSslAbi.DH_set0_pqg(dh,bnP,null,bnG)==1);int codes=-1;Check(OpenSslAbi.DH_check(dh,&codes)==1);Check(codes==int.Parse(oracle(["dh-check",pair.Item1,pair.Item2])));
                    void* borrowed=null;OpenSslAbi.DH_get0_pqg(dh,&borrowed,null,null);Check(borrowed==bnP);void* copy=OpenSslAbi.BN_dup(borrowed);try{Check(OpenSslAbi.BN_sub_word(copy,1)==1&&OpenSslAbi.BN_cmp(copy,borrowed)<0);}finally{OpenSslAbi.BN_free(copy);}
                    void* duplicate=OpenSslAbi.DHparams_dup(dh);Check(duplicate!=null);OpenSslAbi.DH_free(duplicate);
                }
                finally{OpenSslAbi.DH_free(dh);}
            }
            Check(OpenSslAbi.LiveContexts==before);
        });
    }
}
