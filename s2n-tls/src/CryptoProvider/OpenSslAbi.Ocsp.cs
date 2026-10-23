using System.Formats.Asn1;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private sealed record OcspResponseState(OcspResp Value);
    private sealed class BasicResponseState(BasicOcspResp value) : IDisposable
    {
        public readonly BasicOcspResp Value=value;
        public readonly Dictionary<string,nint> Times=new();
        public nint Time(string name,DateTime date){if(Times.TryGetValue(name,out var h))return h;h=TimeValue(date);Times.Add(name,h);return h;}
        public void Dispose(){foreach(var time in Times.Values)Remove((void*)time);Times.Clear();}
    }
    private sealed record CertIdState(CertificateID Value);
    public static void* d2i_OCSP_RESPONSE(void** output,byte** input,long length)
    {
        try
        {
            if(input==null||length<0)throw new ArgumentException();var reader=new AsnReader(Input(*input,(ulong)length).ToArray(),AsnEncodingRules.DER);var encoded=reader.ReadEncodedValue();void* handle=Add(new OcspResponseState(new OcspResp(encoded.ToArray())));
            if(output!=null){if(*output!=null)Remove(*output);*output=handle;}*input+=encoded.Length;return handle;
        }
        catch{Error();return null;}
    }
    public static void OCSP_RESPONSE_free(void* response){try{Remove(response);}catch{Error();}}
    public static int OCSP_response_status(void* response){try{return Get<OcspResponseState>(response).Value.Status;}catch{Error();return -1;}}
    public static void* OCSP_response_get1_basic(void* response)
    {try{return Get<OcspResponseState>(response).Value.GetResponseObject() is BasicOcspResp basic?Add(new BasicResponseState(basic)):null;}catch{Error();return null;}}
    public static void OCSP_BASICRESP_free(void* response){try{Remove(response);}catch{Error();}}
    public static void* OCSP_cert_to_id(void* digest,void* subject,void* issuer)
    {
        try
        {
            var algorithm=digest==null?DigestAlgorithm.Sha1:Get<DigestDescriptor>(digest).Algorithm;
            string oid=algorithm switch{DigestAlgorithm.Md5=>"1.2.840.113549.2.5",DigestAlgorithm.Sha1=>"1.3.14.3.2.26",DigestAlgorithm.Sha224=>"2.16.840.1.101.3.4.2.4",DigestAlgorithm.Sha256=>"2.16.840.1.101.3.4.2.1",DigestAlgorithm.Sha384=>"2.16.840.1.101.3.4.2.2",DigestAlgorithm.Sha512=>"2.16.840.1.101.3.4.2.3",_=>throw new ArgumentException()};
            var s=Get<CertState>(subject).Certificate.Value;var i=Get<CertState>(issuer).Certificate.Value;
            return Add(new CertIdState(new CertificateID(new AlgorithmIdentifier(new DerObjectIdentifier(oid),DerNull.Instance),i,s.SerialNumber)));
        }
        catch{Error();return null;}
    }
    public static void OCSP_CERTID_free(void* id){try{Remove(id);}catch{Error();}}
    public static int OCSP_resp_find_status(void* basic,void* id,int* status,int* reason,void** revocation,void** thisUpdate,void** nextUpdate)
    {
        try
        {
            var b=Get<BasicResponseState>(basic);var target=Get<CertIdState>(id).Value;var responses=b.Value.Responses;
            for(int i=0;i<responses.Length;i++)
            {
                var response=responses[i];if(!response.GetCertID().Equals(target))continue;object state=response.GetCertStatus();
                if(status!=null)*status=state==null?0:state is RevokedStatus?1:2;
                if(state is RevokedStatus revoked)
                {if(reason!=null)*reason=revoked.HasRevocationReason?revoked.RevocationReason:0;if(revocation!=null)*revocation=(void*)b.Time("rev"+i,revoked.RevocationTime);}
                else if(revocation!=null)*revocation=null;
                if(thisUpdate!=null)*thisUpdate=(void*)b.Time("this"+i,response.ThisUpdate);
                if(nextUpdate!=null)*nextUpdate=response.NextUpdate is DateTime date?(void*)b.Time("next"+i,date):null;
                return 1;
            }
            return 0;
        }
        catch{Error();return 0;}
    }
    private static bool VerifyOcspSigner(Org.BouncyCastle.X509.X509Certificate signer,IEnumerable<Org.BouncyCastle.X509.X509Certificate> certificates,StoreState store)
    {
        void* context=null,storeHandle=null,leaf=null,wire=null;
        try
        {
            storeHandle=Add(store.Retain());leaf=Add(new CertState(Certificate.ParseDer(signer.GetEncoded(),out _)));wire=OPENSSL_sk_new_null();
            foreach(var cert in certificates)Get<StackState>(wire).Values.Add((nint)Add(new CertState(Certificate.ParseDer(cert.GetEncoded(),out _))));
            context=X509_STORE_CTX_new();if(X509_STORE_CTX_init(context,storeHandle,leaf,wire)!=1)return false;return X509_verify_cert(context)==1;
        }
        finally
        {
            X509_STORE_CTX_free(context);if(wire!=null){foreach(nint handle in Get<StackState>(wire).Values)X509_free((void*)handle);OPENSSL_sk_free(wire);}X509_free(leaf);X509_STORE_free(storeHandle);
        }
    }
    public static int OCSP_basic_verify(void* basic,void* certs,void* store,ulong flags)
    {
        try
        {
            if(flags!=0)throw new NotSupportedException("s2n requests normal OCSP verification.");var response=Get<BasicResponseState>(basic).Value;var trust=Get<StoreState>(store);
            if(response.Responses.Length==0)throw new ArgumentException("OCSP response has no revocation data.");
            var certificates=response.GetCerts().Concat(certs==null?[]:Get<StackState>(certs).Values.Select(x=>Get<CertState>((void*)x).Certificate.Value)).Concat(trust.Certificates.Select(x=>x.Certificate.Value)).Distinct().ToList();
            foreach(var signer in certificates)
            {
                if(!response.ResponderId.Equals(new RespID(signer.SubjectDN))&&!response.ResponderId.Equals(new RespID(signer.GetPublicKey())))continue;
                if(!response.Verify(signer.GetPublicKey()))continue;if(!VerifyOcspSigner(signer,certificates,trust))continue;
                bool authorized=true;
                foreach(var item in response.Responses)
                {
                    var id=item.GetCertID();bool found=false;
                    foreach(var issuer in certificates)
                    {
                        if(!id.MatchesIssuer(issuer))continue;
                        if(issuer.Equals(signer)){found=true;break;}
                        var eku=signer.GetExtendedKeyUsage();
                        if(eku==null||!eku.Contains(new DerObjectIdentifier("1.3.6.1.5.5.7.3.9"))||!signer.IssuerDN.Equivalent(issuer.SubjectDN,true))continue;
                        try{signer.Verify(issuer.GetPublicKey());found=true;break;}catch(Org.BouncyCastle.Security.GeneralSecurityException){}
                    }
                    if(!found){authorized=false;break;}
                }
                if(authorized)return 1;
            }
            Error();return 0;
        }
        catch{Error();return 0;}
    }
}
