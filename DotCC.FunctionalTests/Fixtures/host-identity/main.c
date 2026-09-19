#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <unistd.h>
#include <errno.h>

int main(void) {
    char name[1024];
    memset(name, 0x5a, sizeof(name));
    if (gethostname(name + 1, sizeof(name) - 2) != 0 || !strlen(name + 1)) return 1;
    if (name[0] != 0x5a || name[1023] != 0x5a) return 2;
    /* Native getlogin_r may have no controlling login. The managed host uses
       its BCL user identity; both must either return an error or a C string. */
    int login = getlogin_r(name + 1, sizeof(name) - 2);
    if (login == 0 && !strlen(name + 1)) return 3;
    if (name[0] != 0x5a || name[1023] != 0x5a) return 4;
    srandom(42);
    long a = random(), b = random();
    srandom(42);
    if (random() != a || random() != b) return 5;
    for (int i = 0; i < 1000; i++) {
        long value = random();
        if (value < 0 || value > 2147483647L) return 6;
    }
    puts("hostname, login buffers and random contract: PASS");
    return 0;
}
