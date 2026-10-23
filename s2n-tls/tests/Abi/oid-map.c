#include <openssl/objects.h>
#include <stdio.h>

static void json_string(const char *text)
{
    putchar('"');if(text)for(;*text;text++){if(*text=='"'||*text=='\\')putchar('\\');if((unsigned char)*text>=32)putchar(*text);}putchar('"');
}
int main(void)
{
    puts("{\"origin\":\"Actual pinned OpenSSL 1.1.1 header/native constant inventory; no runtime native dependency\",\"nids\":[");
    int comma=0;
    for(int nid=0;nid<2000;nid++) {
        const ASN1_OBJECT *object=OBJ_nid2obj(nid);if(!object)continue;
        char oid[512];int count=OBJ_obj2txt(oid,sizeof(oid),object,1);if(count<=0||count>=(int)sizeof(oid))continue;
        int digest=0,key=0;OBJ_find_sigid_algs(nid,&digest,&key);
        if(comma++)puts(",");printf("{\"nid\":%d,\"oid\":",nid);json_string(oid);printf(",\"short_name\":");json_string(OBJ_nid2sn(nid));printf(",\"long_name\":");json_string(OBJ_nid2ln(nid));printf(",\"digest\":%d,\"key\":%d}",digest,key);
    }
    puts("]}");return 0;
}
