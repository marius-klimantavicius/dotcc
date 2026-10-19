#define _DEFAULT_SOURCE 1
#include <netdb.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <errno.h>
int main(void) {
    const char *cases[]={"127.0.0.1","127.1","2130706433","0177.1","0x7f000001","0x7f.1","127.0.0.1 ","127.0.0.1.","::1","::ffff:127.0.0.1","1.2.3.999","999999999999","1x","1a.2",""};
    for(unsigned i=0;i<sizeof cases/sizeof *cases;i++) {
        struct in_addr a; errno=123; h_errno=99;
        struct hostent *h=gethostbyname(cases[i]); int e=errno,he=h_errno;
        int numeric=inet_aton(cases[i],&a);
        printf("NUM [%s] %d %d %d %d",cases[i],h!=NULL,he,e,numeric);
        if(h) printf(" %s %d %d",h->h_name,h->h_addrtype,h->h_length);
        putchar('\n');
        struct hostent out,*r=NULL; char buf[1024]; int er=99; errno=123;
        int status=gethostbyname_r(cases[i],&out,buf,sizeof buf,&r,&er);
        printf("NUMR [%s] %d %d %d %d\n",cases[i],r!=NULL,er,errno,status);
    }
    return 0;
}
