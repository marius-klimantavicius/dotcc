using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Managed.S2nTls.Crypto;
using CryptoCertificate = Managed.S2nTls.Crypto.Certificate;

if (args.Length is <3 or >4) throw new ArgumentException("Usage: ProviderTests <native-vector-exe> <artifact-directory> <expected-native-version-prefix> [case-regex]");
string native = Path.GetFullPath(args[0]), artifacts = Path.GetFullPath(args[1]);
Directory.CreateDirectory(artifacts);
var cases = new List<(string Name, bool Passed, string? Error)>();
var filter=args.Length==4?new System.Text.RegularExpressions.Regex(args[3]):null;int skipped=0;
string Hex(ReadOnlySpan<byte> value) => Convert.ToHexString(value).ToLowerInvariant();
byte[] Bytes(string value) => Convert.FromHexString(value);
string Oracle(params string[] argv)
{
    var start = new ProcessStartInfo(native) { RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var arg in argv) start.ArgumentList.Add(arg);
    using var process = Process.Start(start)!;
    string stdout = process.StandardOutput.ReadToEnd(), stderr = process.StandardError.ReadToEnd();
    if (!process.WaitForExit(30000)) { process.Kill(true); throw new Exception("Native oracle timed out."); }
    if (process.ExitCode != 0) throw new Exception($"Oracle {argv[0]} failed ({process.ExitCode}): {stderr}");
    return stdout.Trim();
}
void Equal(ReadOnlySpan<byte> actual, ReadOnlySpan<byte> expected)
{
    if (!actual.SequenceEqual(expected)) throw new Exception($"Bytes differ: {Hex(actual)} != {Hex(expected)}");
}
void Assert(bool result, string why) { if (!result) throw new Exception(why); }
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or CryptographicException or InvalidOperationException) { return; }
    throw new Exception("Expected operation to reject input.");
}
void Test(string name, Action test)
{
    if(filter!=null&&!filter.IsMatch(name)){skipped++;return;}
    try { test(); cases.Add((name, true, null)); Console.WriteLine($"PASS {name}"); }
    catch (Exception e) { cases.Add((name, false, e.ToString())); Console.WriteLine($"FAIL {name}: {e.Message}"); }
}
string version = Oracle("version");
Assert(version.StartsWith(args[2], StringComparison.Ordinal), "Native oracle version does not match the pinned metadata.");
byte[] message = Enumerable.Range(0, 319).Select(i => (byte)(i * 31 + 7)).ToArray();
foreach (var algorithm in Enum.GetValues<DigestAlgorithm>())
{
    string name = algorithm switch { DigestAlgorithm.Md5Sha1 => "md5-sha1",DigestAlgorithm.Shake256_64=>"shake256-64", _ => algorithm.ToString().ToLowerInvariant() };
    Test($"digest/{name}/clone-reset-native", () =>
    {
        using var digest = new Digest(algorithm); digest.Update(message.AsSpan(0, 63));
        using var clone = digest.Clone(); digest.Update(message.AsSpan(63)); clone.Update(message.AsSpan(63));
        byte[] expected = Bytes(Oracle("hash", name, Hex(message)));
        Equal(digest.FinalizeHash(), expected); Equal(clone.FinalizeHash(), expected);
        Reject(() => digest.Update([1])); digest.Reset(); digest.Update(message); Equal(digest.FinalizeHash(), expected);
        using var branch = new Digest(algorithm); branch.Update(message.AsSpan(0, 63)); using var fork = branch.Clone();
        branch.Update([0]); fork.Update([1]);
        Equal(branch.FinalizeHash(), Bytes(Oracle("hash", name, Hex([.. message.AsSpan(0, 63), 0]))));
        Equal(fork.FinalizeHash(), Bytes(Oracle("hash", name, Hex([.. message.AsSpan(0, 63), 1]))));
        Equal(Digest.Hash(algorithm, []), Bytes(Oracle("hash", name, "")));
    });
}
Test("rc4/rfc6229-and-continuing-records", () =>
{
    byte[] key = Bytes("0102030405"), zeros = new byte[272], encrypted = new byte[272];
    using var cipher = new Rc4(key); cipher.Process(zeros.AsSpan(0, 17), encrypted.AsSpan(0, 17)); cipher.Process(zeros.AsSpan(17), encrypted.AsSpan(17));
    Equal(encrypted.AsSpan(0, 16), Bytes("b2396305f03dc027ccc3524a0a1118a8"));
    Equal(encrypted.AsSpan(256, 16), Bytes("1cfcf62b03eddb641d77dfcf7f8d8c93"));
    key = Enumerable.Range(0, 16).Select(x => (byte)x).ToArray();
    using var nativeSized = new Rc4(key); nativeSized.Process(message.AsSpan(0, 27), encrypted.AsSpan(0, 27));
    byte[] all = new byte[message.Length]; encrypted.AsSpan(0,27).CopyTo(all); nativeSized.Process(message.AsSpan(27), all.AsSpan(27));
    Equal(all, Bytes(Oracle("cipher", "rc4", Hex(key), "", Hex(message), "27", "1")));
});
foreach (var (name, keyLength, block) in new[] { ("aes-128-cbc", 16, 16), ("aes-256-cbc", 32, 16), ("des-ede3-cbc", 24, 8) })
{
    Test($"cbc/{name}/no-padding-chain-inplace", () =>
    {
        byte[] key = Enumerable.Range(0, keyLength).Select(x => (byte)(x * 7 + 1)).ToArray();
        byte[] iv = Enumerable.Range(0, block).Select(x => (byte)(x + 3)).ToArray(), input = message[..(message.Length / block * block)], output = new byte[input.Length];
        using var cipher = new CbcCipher(block == 8, true, key, iv);
        cipher.Process(input.AsSpan(0, block), output.AsSpan(0, block)); cipher.Process(input.AsSpan(block), output.AsSpan(block));
        Equal(output, Bytes(Oracle("cipher", name, Hex(key), Hex(iv), Hex(input), block.ToString(), "1")));
        Equal(cipher.GetIv(), output.AsSpan(output.Length - block));
        using var decrypt = new CbcCipher(block == 8, false, key, iv);
        byte[] copy = (byte[])output.Clone(); decrypt.Process(copy.AsSpan(0, block), copy.AsSpan(0, block)); decrypt.Process(copy.AsSpan(block), copy.AsSpan(block));
        Equal(copy, input); Equal(decrypt.GetIv(), output.AsSpan(output.Length - block));
        Equal(Bytes(Oracle("cipher", name, Hex(key), Hex(iv), Hex(output), block.ToString(), "0")), input);
        Reject(() => cipher.Process([1], new byte[1]));
    });
}
Test("3des/weak-key-native", () =>
{
    var key = new byte[24]; var iv = new byte[8]; var input = new byte[16]; var output = new byte[16];
    using var cipher = new CbcCipher(true, true, key, iv); cipher.Process(input, output);
    Equal(output, Bytes(Oracle("cipher", "des-ede3-cbc", Hex(key), Hex(iv), Hex(input), "8", "1")));
});
foreach (var (name, algorithm, length) in new[] { ("aes-128-gcm", AeadAlgorithm.AesGcm, 16), ("aes-256-gcm", AeadAlgorithm.AesGcm, 32), ("chacha20-poly1305", AeadAlgorithm.ChaCha20Poly1305, 32) })
{
    Test($"aead/{name}/native-reject-empty", () =>
    {
        byte[] key = Enumerable.Range(0,length).Select(x=>(byte)x).ToArray(), nonce = Enumerable.Range(0,12).Select(x=>(byte)(x+2)).ToArray(), aad = message[..13];
        foreach (byte[] input in new[] { message, Array.Empty<byte>() })
        {
            byte[] ciphertext = new byte[input.Length], tag = new byte[16], plaintext = new byte[input.Length];
            Aead.Encrypt(algorithm,key,nonce,input,aad,ciphertext,tag);
            Equal([..ciphertext,..tag], Bytes(Oracle("aead",name,Hex(key),Hex(nonce),Hex(input),Hex(aad),"-")));
            Assert(Aead.TryDecrypt(algorithm,key,nonce,ciphertext,aad,tag,plaintext), "Valid AEAD rejected."); Equal(plaintext,input);
            tag[0]^=1; plaintext.AsSpan().Fill(0xff);
            Assert(!Aead.TryDecrypt(algorithm,key,nonce,ciphertext,aad,tag,plaintext), "Bad tag accepted."); Equal(plaintext,new byte[input.Length]);
            Assert(Oracle("aead",name,Hex(key),Hex(nonce),Hex(ciphertext),Hex(aad),Hex(tag))=="REJECT", "Native accepted bad tag.");
        }
    });
}
foreach (int bits in new[] { 512, 1024, 2048 })
{
    Test($"rsa/{bits}/encoding-pkcs1-legacy-pss", () =>
    {
        using var raw = RSA.Create(bits); using var key = RsaKey.FromPem(raw.ExportRSAPrivateKeyPem());
        string priv = Path.Combine(artifacts,$"rsa-{bits}.pem"), pub = Path.Combine(artifacts,$"rsa-{bits}.pub.pem");
        File.WriteAllText(priv,raw.ExportPkcs8PrivateKeyPem()); File.WriteAllText(pub,raw.ExportSubjectPublicKeyInfoPem());
        using var publicKey = RsaKey.FromPublicDer(key.ExportPublicDer()); using var derKey = RsaKey.FromPrivateDer(key.ExportPrivateDer());
        Equal(publicKey.ExportPublicDer(),derKey.ExportPublicDer());
        foreach (var algorithm in Enum.GetValues<DigestAlgorithm>())
        {
            // SHAKE is an XOF used by the selected hash API, not an RSA DigestInfo.
            if(algorithm==DigestAlgorithm.Shake256_64)continue;
            // 512-bit RSA cannot encode SHA384/SHA512 DigestInfo under PKCS1.
            if (bits==512 && algorithm is DigestAlgorithm.Sha384 or DigestAlgorithm.Sha512) continue;
            string md=algorithm==DigestAlgorithm.Md5Sha1?"md5-sha1":algorithm.ToString().ToLowerInvariant();
            byte[] hash=Digest.Hash(algorithm,message), sig=key.SignHash(algorithm,hash);
            Equal(sig,Bytes(Oracle("rsa-sign",priv,md,"pkcs1",Hex(hash))));
            Assert(publicKey.VerifyHash(algorithm,hash,sig),"Managed signature rejected.");
            Assert(Oracle("rsa-verify",pub,md,"pkcs1",Hex(hash),Hex(sig))=="1","Native signature rejected.");
            sig[0]^=1; Assert(!publicKey.VerifyHash(algorithm,hash,sig),"Corrupt signature accepted.");
            if (bits >= 1024 && algorithm is DigestAlgorithm.Sha256 or DigestAlgorithm.Sha384 || bits==2048 && algorithm==DigestAlgorithm.Sha512)
            {
                sig=key.SignHash(algorithm,hash,true); Assert(Oracle("rsa-verify",pub,md,"pss",Hex(hash),Hex(sig))=="1","Native PSS verify rejected.");
                byte[] nativeSig=Bytes(Oracle("rsa-sign",priv,md,"pss",Hex(hash))); Assert(publicKey.VerifyHash(algorithm,hash,nativeSig,true),"Native PSS rejected by managed.");
            }
        }
        byte[] data=message[..Math.Min(48,key.Size-11)], encrypted=publicKey.EncryptPkcs1(data), clear=new byte[key.Size];
        Equal(Bytes(Oracle("rsa-decrypt",priv,Hex(encrypted))),data);
        encrypted=Bytes(Oracle("rsa-encrypt",pub,Hex(data)));Assert(key.TryDecryptPkcs1(encrypted,clear,out int count),"RSA decryption failed.");Equal(clear.AsSpan(0,count),data);
        Assert(!key.TryDecryptPkcs1(new byte[key.Size],clear,out count)&&count==0,"Invalid PKCS1 ciphertext accepted.");
    });
}
Test("dh/2048-native-invalid-peer", () =>
{
    // RFC3526 group 14. The existing upstream fixture supplies exact PKCS3 parameters.
    byte[] p=Bytes("FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AACAA68FFFFFFFFFFFFFFFF"), g=[2];
    var alice=new FiniteFieldDh(p,g);var bob=new FiniteFieldDh(p,g);byte[] x=Bytes("1234567890abcdef1234567890abcdef"), ap=alice.ImportPrivateKey(x), bp=bob.GeneratePublicKey();
    Equal(alice.Derive(bp),bob.Derive(ap));Equal(alice.Derive(bp),Bytes(Oracle("dh",Hex(p),Hex(g),Hex(x),Hex(bp))));
    Reject(()=>alice.Derive([0]));Reject(()=>alice.Derive([1]));Reject(()=>alice.Derive(p));
});
foreach (string curve in new[] { "P-256", "P-384", "P-521", "X25519" })
{
    Test($"ecdh/{curve}/native-encoding-reject",()=>
    {
        using var alice=new EcKeyExchange(curve);using var bob=new EcKeyExchange(curve);byte[] secret=alice.Derive(bob.ExportPoint());Equal(secret,bob.Derive(alice.ExportPoint()));
        string priv=Path.Combine(artifacts,$"ecdh-{curve}.pem"),pub=Path.Combine(artifacts,$"ecdh-{curve}.pub.pem");File.WriteAllText(priv,alice.ExportPrivatePem());File.WriteAllText(pub,bob.ExportPublicPem());
        Equal(secret,Bytes(Oracle("ecdh",priv,pub)));Reject(()=>alice.Derive(new byte[bob.ExportPoint().Length]));
    });
    if(curve=="X25519")continue;
    Test($"ecdsa/{curve}/native-der-signature",()=>
    {
        using var raw=ECDsa.Create(EcKeyExchange.Curve(curve));using var key=EcdsaKey.FromPem(raw.ExportECPrivateKeyPem());byte[] hash=Digest.Hash(DigestAlgorithm.Sha256,message),sig=key.SignHash(hash);
        string priv=Path.Combine(artifacts,$"ecdsa-{curve}.pem"),pub=Path.Combine(artifacts,$"ecdsa-{curve}.pub.pem");File.WriteAllText(priv,raw.ExportPkcs8PrivateKeyPem());File.WriteAllText(pub,raw.ExportSubjectPublicKeyInfoPem());
        Assert(Oracle("ecdsa-verify",pub,"sha256","none",Hex(hash),Hex(sig))=="1","Native ECDSA rejected.");
        Assert(key.VerifyHash(hash,Bytes(Oracle("ecdsa-sign",priv,"sha256","none",Hex(hash)))),"Native ECDSA signature rejected.");
        sig[^1]^=1;Assert(!key.VerifyHash(hash,sig),"Corrupt ECDSA signature accepted.");Equal(key.ExportPublicDer(),raw.ExportSubjectPublicKeyInfo());
    });
}
Test("certificate/pem-der-spki-chain",()=>
{
    using var rootKey=RSA.Create(2048);var rootRequest=new CertificateRequest("CN=Provider test root",rootKey,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
    rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true,false,0,true));rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign,true));
    var now=DateTimeOffset.UtcNow;using var root=rootRequest.CreateSelfSigned(now.AddDays(-1),now.AddDays(3));using var leafKey=RSA.Create(1024);
    var request=new CertificateRequest("CN=provider.example",leafKey,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false,false,0,true));
    using var leaf=request.Create(root,now.AddHours(-1),now.AddDays(1),[1,2,3,4]);
    string pem=leaf.ExportCertificatePem(),file=Path.Combine(artifacts,"leaf.pem");File.WriteAllText(file,pem);
    var cert=CryptoCertificate.ParsePem(pem);Equal(cert.Der,Bytes(Oracle("cert",file)));Equal(cert.PublicKeyDer,leafKey.ExportSubjectPublicKeyInfo());
    var decoded=CryptoCertificate.ParseDer([..cert.Der,0x00],out int consumed);Assert(consumed==cert.Der.Length,"DER consumed wrong number of bytes.");Equal(decoded.Der,cert.Der);
    var anchor=CryptoCertificate.ParsePem(root.ExportCertificatePem());Assert(cert.VerifySignature(anchor),"Certificate signature rejected.");
    Assert(CryptoCertificate.VerifyChain([cert,anchor],[anchor],now.UtcDateTime),"Valid chain rejected.");Assert(!CryptoCertificate.VerifyChain([cert],[anchor],now.AddDays(5).UtcDateTime),"Expired chain accepted.");
    Assert(!CryptoCertificate.VerifyChain([cert],[],now.UtcDateTime),"Untrusted chain accepted.");Reject(()=>CryptoCertificate.ParseDer([0x30,0x80,0,0],out _));
});
Base64Checks.Run(Test,p=>Oracle(p));
Asn1StringChecks.Run(Test,p=>Oracle(p));
RsaPrivateChecks.Run(Test,p=>Oracle(p),artifacts,args[2]);
EcPublicChecks.Run(Test,p=>Oracle(p),artifacts,args[2]);
AbiChecks.Run(Test,p=>Oracle(p),artifacts);
CertificateChecks.Run(Test,p=>Oracle(p),artifacts);
if(cases.Count==0)throw new ArgumentException("Case filter selected no tests.");
int failures=cases.Count(x=>!x.Passed);
using(var report = File.Create(Path.Combine(artifacts,"managed-report.json")))
using(var writer = new Utf8JsonWriter(report,new JsonWriterOptions{Indented=true}))
{
    writer.WriteStartObject();writer.WriteString("scope","independent managed provider / pinned native primitive vectors; emitted s2n ABI and TLS not exercised");
    writer.WriteString("native",version);writer.WriteNumber("passed",cases.Count-failures);writer.WriteNumber("failed",failures);writer.WriteStartArray("cases");
    foreach(var item in cases)
    { writer.WriteStartObject();writer.WriteString("name",item.Name);writer.WriteBoolean("passed",item.Passed);writer.WriteString("error",item.Error);writer.WriteEndObject(); }
    writer.WriteEndArray();writer.WriteNumber("unselected",skipped);if(filter!=null)writer.WriteString("case_filter",args[3]);writer.WriteEndObject();
}
Console.WriteLine($"{cases.Count-failures} passed; {failures} failed. Report: {Path.Combine(artifacts,"managed-report.json")}");
return failures==0?0:1;
