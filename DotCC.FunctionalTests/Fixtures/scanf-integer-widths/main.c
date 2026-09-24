#include <stdio.h>
#include <stdint.h>
int main(void) {
    unsigned int major=0, minor=0, patch=0;
    int count=sscanf("5.4.7", "%u.%u.%u", &major, &minor, &patch);
    printf("%d %u %u %u\n",count,major,minor,patch);
    int type=0; unsigned long a=0,b=0,c=0;
    count=sscanf("2,4294967296,18446744073709551615,71", "%i,%lu,%lu,%lu", &type,&a,&b,&c);
    printf("%d %d %lu %lu %lu\n",count,type,a,b,c);
    long long signedvalue=0; unsigned char byte=0; short small=0; unsigned short usmall=0;
    count=sscanf("-9223372036854775808 FF -32768 65535", "%lld %02hhX %hd %hu", &signedvalue,&byte,&small,&usmall);
    printf("%d %lld %u %d %u\n",count,signedvalue,byte,small,usmall);
    int hex=0,octal=0,decimal=0; unsigned int uhex=0;
    count=sscanf("0xAB 077 -19 0Xff", "%i %i %i %x", &hex,&octal,&decimal,&uhex);
    printf("%d %d %d %d %u\n",count,hex,octal,decimal,uhex);
    a=7;b=8;c=9;
    count=sscanf("11;22,33", "%lu,%lu,%lu", &a,&b,&c);
    printf("%d %lu %lu %lu\n",count,a,b,c);
    int width=0,last=0;
    count=sscanf("skip 12345%7", "%*s %3d%*2d%%%d", &width,&last);
    printf("%d %d %d\n",count,width,last);
    return 0;
}
