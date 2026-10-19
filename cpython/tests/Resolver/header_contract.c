#define _DEFAULT_SOURCE 1
#include <netdb.h>
#include <stddef.h>
#include <netinet/in.h>
static int (*reverse_call)(const struct sockaddr *, socklen_t, char *, socklen_t, char *, socklen_t, int) = getnameinfo;
static struct hostent *(*host_call)(const char *) = gethostbyname;
_Static_assert(sizeof(socklen_t)==4, "socklen_t uint32");
_Static_assert(sizeof(((struct addrinfo *)0)->ai_addrlen)==4, "addrlen uint32");
_Static_assert(sizeof(struct addrinfo)==48 && _Alignof(struct addrinfo)==8, "addrinfo layout");
_Static_assert(offsetof(struct addrinfo,ai_addrlen)==16 && offsetof(struct addrinfo,ai_addr)==24 && offsetof(struct addrinfo,ai_canonname)==32 && offsetof(struct addrinfo,ai_next)==40, "addrinfo field order");
_Static_assert(sizeof(struct hostent)==32 && _Alignof(struct hostent)==8, "hostent layout");
_Static_assert(offsetof(struct hostent,h_name)==0 && offsetof(struct hostent,h_aliases)==8 && offsetof(struct hostent,h_addrtype)==16 && offsetof(struct hostent,h_length)==20 && offsetof(struct hostent,h_addr_list)==24, "hostent field order");
_Static_assert(NI_NUMERICHOST==1 && NI_NUMERICSERV==2 && NI_NOFQDN==4 && NI_NAMEREQD==8 && NI_DGRAM==16, "NI values");
int main(void) { h_errno=99; return reverse_call==0 || host_call==0 || h_errno!=99; }
