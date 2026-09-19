#include <netdb.h>
#include <errno.h>
#include <string.h>
#include <stddef.h>
#include <stdio.h>
int main(void) {
    printf("abi=%zu,%zu,%zu,%zu\n", sizeof(struct protoent), _Alignof(struct protoent), offsetof(struct protoent, p_aliases), offsetof(struct protoent, p_proto));
    printf("errno=%d,%d,%d,%d,%d,%d,%d\n", ETXTBSY, ENOTEMPTY, ELOOP, ENODATA, ENOLINK, ENETRESET, EWOULDBLOCK);
    printf("messages=%s;%s;%s;%s;%s;%s\n", strerror(ETXTBSY), strerror(ENOTEMPTY), strerror(ELOOP), strerror(ENODATA), strerror(ENOLINK), strerror(ENETRESET));
    struct protoent *p = getprotobyname("tcp");
    if (!p) return 1;
    printf("tcp=%s,%d,%s,%d\n", p->p_name, p->p_proto, p->p_aliases[0], p->p_aliases[1] == NULL);
    p = getprotobyname("TCP"); if (!p) return 2;
    printf("alias=%s,%d\n", p->p_name, p->p_proto);
    p = getprotobyname("udp"); if (!p) return 3;
    printf("udp=%s,%d\n", p->p_name, p->p_proto);
    printf("direct=%d\n", getprotobyname("tcp")->p_proto);
    printf("unknown=%d\n", getprotobyname("dotcc-unknown-protocol") == NULL);
    return 0;
}
