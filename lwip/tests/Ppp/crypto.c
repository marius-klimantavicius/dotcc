#include "lwip/init.h"
#include "lwip/pbuf.h"
#include "netif/ppp/ppp_impl.h"
#include "netif/ppp/chap-new.h"
#include "netif/ppp/chap_ms.h"
#include "netif/ppp/mppe.h"
#include "netif/ppp/polarssl/md4.h"
#include "netif/ppp/polarssl/md5.h"
#include "netif/ppp/polarssl/sha1.h"
#include "netif/ppp/polarssl/des.h"
#include "netif/ppp/polarssl/arc4.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#define REQUIRE(x) do { if (!(x)) { fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#x); exit(92); } } while (0)
static void unhex(const char *s,unsigned char *out) { for(size_t i=0;i<strlen(s)/2;i++){unsigned x;REQUIRE(sscanf(s+2*i,"%2x",&x)==1);out[i]=(unsigned char)x;} }
static void equalhex(const unsigned char *data,size_t length,const char *hex) { unsigned char expected[64];REQUIRE(length<=sizeof(expected));unhex(hex,expected);REQUIRE(!memcmp(data,expected,length)); }
static void primitives(void)
{
  unsigned char digest[20],input[]="abc";md4(input,3,digest);equalhex(digest,16,"a448017aaf21d8525fc10ae87aa6729d");
  md5(input,3,digest);equalhex(digest,16,"900150983cd24fb0d6963f7d28e17f72");
  sha1(input,3,digest);equalhex(digest,20,"a9993e364706816aba3e25717850c26c9cd0d89d");
  unsigned char key[8],plain[8],cipher[8],back[8];unhex("133457799bbcdff1",key);unhex("0123456789abcdef",plain);des_context des;
  des_setkey_enc(&des,key);des_crypt_ecb(&des,plain,cipher);equalhex(cipher,8,"85e813540f0ab405");
  des_setkey_dec(&des,key);des_crypt_ecb(&des,cipher,back);REQUIRE(!memcmp(plain,back,8));
  arc4_context rc4;unsigned char stream[]="Plaintext",streamkey[]="Key";arc4_setup(&rc4,streamkey,3);arc4_crypt(&rc4,stream,9);equalhex(stream,9,"bbf316e8d940af0ad3");
}
static void chap(ppp_pcb *pcb)
{
  /* RFC2759 section9.2 authenticates a supplied peer response; all calculations
   * occur in the original bundled digest callback, including MPPE key setup. */
  unsigned char challenge[17]={16},response[50]={49};char message[256];
  unhex("5b5d7c7d7b3f2f3e3c2c602132262628",challenge+1);
  unhex("21402324255e262a28295f2b3a337c7e",response+1);
  unhex("82309ecd8d708b5ea08faa3981cd83544233114a3d85d6df",response+25);
  REQUIRE(chapms2_digest.verify_response(pcb,7,"User",(const unsigned char*)"clientPass",10,challenge,response,message,sizeof(message))==1);
  REQUIRE(!strncmp(message,"S=407A5589115FD0D6209F510FE9C04566932CDA56",42));
}
static void mppe_cases(ppp_pcb *pcb)
{
  unsigned char key[16];unhex("8b7cdc149b993a1ba118cb153f56dccb",key);
  for(int stateful=0;stateful<2;stateful++) {
    ppp_mppe_state enc={0},dec={0};mppe_set_key(pcb,&enc,key);mppe_set_key(pcb,&dec,key);
    unsigned options=MPPE_OPT_128|(stateful?MPPE_OPT_STATEFUL:0);mppe_init(pcb,&enc,options);mppe_init(pcb,&dec,options);
    equalhex(enc.session_key,16,"405cb2247a7956e6e211007ae27b22d4");
    for(unsigned packet=0;packet<300;packet++) {
      unsigned char bytes[73],actual[75];for(unsigned i=0;i<73;i++)bytes[i]=(unsigned char)(packet+i*7);
      struct pbuf *original=pbuf_alloc(PBUF_RAW,73,PBUF_RAM);REQUIRE(original && pbuf_take(original,bytes,73)==ERR_OK);
      struct pbuf *wire=original;REQUIRE(mppe_compress(pcb,&enc,&wire,0x21)==ERR_OK && wire!=original);
      REQUIRE(pbuf_copy_partial(original,actual,73,0)==73 && !memcmp(actual,bytes,73));pbuf_free(original);
      REQUIRE(mppe_decompress(pcb,&dec,&wire)==ERR_OK && wire->tot_len==75);
      REQUIRE(pbuf_copy_partial(wire,actual,75,0)==75 && actual[0]==0 && actual[1]==0x21 && !memcmp(actual+2,bytes,73));pbuf_free(wire);
    }
  }
}
int main(int argc,char **argv)
{
  if(argc>1 && !strcmp(argv[1],"--negative"))REQUIRE(0);
  lwip_init();primitives();ppp_pcb pcb={0};struct netif netif={0};pcb.netif=&netif;
  chap(&pcb);mppe_cases(&pcb);
  puts("{\"primitives\":5,\"chap_rfc2759_positive\":1,\"mppe_packets\":600}");return 0;
}
