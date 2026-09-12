#include <stdio.h>
#include <netinet/in.h>
#include <arpa/inet.h>
const struct in6_addr *other_loopback(void);
int main(void) {
    struct in6_addr any = IN6ADDR_ANY_INIT;
    struct in6_addr loopback = IN6ADDR_LOOPBACK_INIT;
    struct in6_addr copy = in6addr_loopback;
    char text[INET6_ADDRSTRLEN];
    copy.s6_addr[15] = 9;
    printf("%d %d %d %d %d %d\n", any.s6_addr[15], loopback.s6_addr[15],
        copy.s6_addr[15], in6addr_loopback.s6_addr[15], &in6addr_any != &in6addr_loopback, other_loopback() == &in6addr_loopback);
    inet_ntop(AF_INET6, &in6addr_any, text, sizeof(text));
    printf("%s\n", text);
    inet_ntop(AF_INET6, &in6addr_loopback, text, sizeof(text));
    printf("%s\n", text);
    return 0;
}
