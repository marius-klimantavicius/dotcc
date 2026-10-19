/* B25 reduction of the unchanged header_contract.c function-pointer RED.
   Same public C header and service; no private or replacement declaration. */
#define _DEFAULT_SOURCE 1
#include <netdb.h>
#include <errno.h>
#include <stdio.h>

#ifndef DIRECT_CALL
static struct hostent *(*const lookup)(const char *) = gethostbyname;
#endif

int main(void)
{
    errno = 123;
    h_errno = 99;
#ifdef DIRECT_CALL
    struct hostent *h = gethostbyname("127.0.0.1");
#else
    struct hostent *h = lookup("127.0.0.1");
#endif
    if (!h || h->h_addrtype != 2 || h->h_length != 4 || !h->h_addr_list ||
        !h->h_addr_list[0] || h->h_addr_list[1]) return 51;
    const unsigned char *a = (const unsigned char *)h->h_addr_list[0];
    printf("typed-host %d %d %u.%u.%u.%u h_errno=%d errno=%d\n",
           h->h_addrtype, h->h_length, a[0], a[1], a[2], a[3], h_errno, errno);
    return a[0] == 127 && a[1] == 0 && a[2] == 0 && a[3] == 1 &&
           h_errno == 99 && errno == 123 ? 0 : 52;
}
