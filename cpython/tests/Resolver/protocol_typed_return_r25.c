/* Independent public-header sibling for B25's typed runtime return mismatch. */
#define _DEFAULT_SOURCE 1
#include <netdb.h>
#include <stdio.h>
static struct protoent *(*const lookup)(const char *) = getprotobyname;
int main(void)
{
    struct protoent *p = lookup("tcp");
    if (!p) return 53;
    printf("typed-protocol %s %d\n", p->p_name, p->p_proto);
    return p->p_proto == 6 ? 0 : 54;
}
