using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private static Asn1Object? ExtensionValue(X509ExtensionBase item,DerObjectIdentifier oid)
    {var value=item.GetExtensionValue(oid);return value==null?null:Asn1Object.FromByteArray(value.GetOctets());}
    private static bool HasUnhandledCertificateCriticalExtension(X509Certificate certificate)
    {
        // OpenSSL 1.1.1w X509_supported_extension, using the selected header
        // configuration (RFC3779 enabled). This is recognition, not validation:
        // path constraints are still checked by the real PKIX validator.
        foreach(string oid in certificate.GetCriticalExtensionOids()??new HashSet<string>())
            if(oid is not("2.16.840.1.113730.1.1" or "2.5.29.15" or "2.5.29.17" or "2.5.29.19" or "2.5.29.32" or "2.5.29.31" or "2.5.29.37" or "1.3.6.1.5.5.7.1.7" or "1.3.6.1.5.5.7.1.8" or "2.5.29.36" or "1.3.6.1.5.5.7.1.14" or "2.5.29.30" or "2.5.29.33" or "2.5.29.54"))return true;
        return false;
    }
    private static int CheckAuthorityIdentifier(X509Certificate issuer,AuthorityKeyIdentifier? authority)
    {
        if(authority==null)return 0;
        var ski=ExtensionValue(issuer,X509Extensions.SubjectKeyIdentifier);
        if(authority.KeyIdentifier is { } key&&ski!=null&&!key.GetOctets().AsSpan().SequenceEqual(SubjectKeyIdentifier.GetInstance(ski).GetKeyIdentifier()))return 30;
        if(authority.AuthorityCertSerialNumber is { } serial&&!serial.Equals(issuer.SerialNumber))return 31;
        var name=authority.AuthorityCertIssuer?.GetNames().FirstOrDefault(x=>x.TagNo==GeneralName.DirectoryName);
        if(name!=null&&!X509Name.GetInstance(name.Name).Equivalent(issuer.IssuerDN,true))return 31;
        return 0;
    }
    private static GeneralName[] DistributionNames(DistributionPointName point,X509Name issuer)
    {
        if(point.Type==DistributionPointName.FullName)return GeneralNames.GetInstance(point.Name).GetNames();
        var parts=Asn1Sequence.GetInstance(issuer.ToAsn1Object()).Cast<Asn1Encodable>().Append(point.Name).ToArray();
        return [new GeneralName(GeneralName.DirectoryName,X509Name.GetInstance(new DerSequence(parts)))];
    }
    private static bool DistributionMatch(DistributionPointName? left,DistributionPointName? right,X509Name issuer)
    {
        if(left==null||right==null)return true;
        return DistributionNames(left,issuer).Any(a=>DistributionNames(right,issuer).Any(b=>a.TagNo==b.TagNo&&(a.TagNo==GeneralName.DirectoryName?X509Name.GetInstance(a.Name).Equivalent(X509Name.GetInstance(b.Name),true):a.Equals(b))));
    }
    private static bool CrlScope(X509Certificate certificate,X509Crl crl,IssuingDistributionPoint? scope,out bool allReasons)
    {
        allReasons=true;
        if(scope!=null&&(scope.OnlyContainsAttributeCerts||scope.OnlyContainsCACerts&&certificate.GetBasicConstraints()<0||scope.OnlyContainsUserCerts&&certificate.GetBasicConstraints()>=0))return false;
        var points=ExtensionValue(certificate,X509Extensions.CrlDistributionPoints);
        if(points!=null)foreach(var point in CrlDistPoint.GetInstance(points).GetDistributionPoints())
        {
            bool issuer=point.CrlIssuer==null||point.CrlIssuer.GetNames().Any(x=>x.TagNo==GeneralName.DirectoryName&&X509Name.GetInstance(x.Name).Equivalent(crl.IssuerDN,true));
            if(issuer&&DistributionMatch(point.DistributionPointName,scope?.DistributionPoint,crl.IssuerDN))
            {allReasons=point.Reasons==null||(point.Reasons.IntValue&0x807f)==0x807f;return true;}
        }
        return scope?.DistributionPoint==null;
    }
    private static bool CrlCritical(X509Crl crl)
    {
        foreach(string oid in crl.GetCriticalExtensionOids()??new HashSet<string>())if(oid is not("2.5.29.28" or "2.5.29.35" or "2.5.29.27"))return true;
        foreach(var entry in crl.GetRevokedCertificates()??new HashSet<X509CrlEntry>())
            foreach(string oid in entry.GetCriticalExtensionOids()??new HashSet<string>())if(oid!="2.5.29.29")return true;
        return false;
    }
    private static bool VerifyCrls(StoreContext c,void* ctx,List<CertState> path,DateTime time)
    {
        int count=(c.Param.Flags&CrlCheckAll)!=0?path.Count:1;
        for(int i=0;i<count;i++)
        {
            var cert=path[i].Certificate.Value;X509Crl? crl=null;X509Certificate? issuer=null;int best=0;bool scopeOk=false,allReasons=true,critical=false;
            foreach(var candidate in c.Crls)
            {
                var value=candidate.Value;if(!value.IssuerDN.Equivalent(cert.IssuerDN,true))continue;
                var scope=IssuingDistributionPoint.GetInstance(ExtensionValue(value,X509Extensions.IssuingDistributionPoint));
                // s2n never enables EXTENDED_CRL_SUPPORT. The pinned native
                // default rejects indirect and reason-partitioned CRLs.
                if(scope!=null&&(scope.IsIndirectCrl||scope.OnlySomeReasons!=null||(Convert.ToInt32(scope.OnlyContainsUserCerts)+Convert.ToInt32(scope.OnlyContainsCACerts)+Convert.ToInt32(scope.OnlyContainsAttributeCerts)>1)))continue;
                var authority=AuthorityKeyIdentifier.GetInstance(ExtensionValue(value,X509Extensions.AuthorityKeyIdentifier));
                var signer=path.Skip(Math.Min(i+1,path.Count-1)).Select(x=>x.Certificate.Value).FirstOrDefault(x=>x.SubjectDN.Equivalent(value.IssuerDN,true)&&CheckAuthorityIdentifier(x,authority)==0);
                if(signer==null)continue;
                bool validScope=CrlScope(cert,value,scope,out bool complete),hasCritical=CrlCritical(value);
                int score=4+(!hasCritical?256:0)+(validScope?128:0)+((c.Param.Flags&NoCheckTime)!=0||time>=value.ThisUpdate&&(value.NextUpdate==null||time<=value.NextUpdate)?64:0);
                if(score<best||score==best&&crl!=null&&value.ThisUpdate<=crl.ThisUpdate)continue;
                best=score;crl=value;issuer=signer;scopeOk=validScope;allReasons=complete;critical=hasCritical;
            }
            int Fail(int error){VerificationError(c,ctx,error);return 0;}
            if(crl==null||issuer==null){Fail(3);return false;}
            bool delta=ExtensionValue(crl,X509Extensions.DeltaCrlIndicator)!=null;
            // Match this pin's default behavior: a standalone delta still
            // participates, but base/delta merging requires USE_DELTAS.
            bool[]? usage=issuer.GetKeyUsage();if(!delta&&usage!=null&&(usage.Length<=6||!usage[6])){Fail(35);return false;}
            if(!delta&&!scopeOk){Fail(44);return false;}
            int timeError=time<crl.ThisUpdate?11:crl.NextUpdate is DateTime expiry&&time>expiry?12:0;
            if(timeError!=0&&(c.Param.Flags&NoCheckTime)==0)
            {c.Error=timeError;if(c.Callback==null||c.Callback(0,ctx)!=1){Fail(timeError);return false;}c.CallbackError=timeError;c.Error=0;}
            try{crl.Verify(issuer.GetPublicKey());}catch{Fail(8);return false;}
            if(critical){Fail(36);return false;}
            var revoked=crl.GetRevokedCertificate(cert.SerialNumber);
            if(revoked!=null)
            {
                var reason=ExtensionValue(revoked,X509Extensions.ReasonCode);
                if(reason==null||DerEnumerated.GetInstance(reason).IntValueExact!=8){Fail(23);return false;}
            }
            if(!allReasons){Fail(3);return false;}
        }
        return true;
    }
}
