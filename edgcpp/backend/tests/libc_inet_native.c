#include <arpa/inet.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
int main(void) {
    char line[256];
    while (fgets(line, sizeof(line), stdin)) {
        char *text = strchr(line, '\t');
        int family = atoi(line), result, error;
        unsigned char output[20];
        if (!text) return 2;
        ++text; text[strcspn(text, "\n")] = 0;
        memset(output, 0xa5, sizeof(output)); errno = 123;
        result = inet_pton(family == 2 ? AF_INET : family == 10 ? AF_INET6 : -1, text, output + 2);
        error = errno == EAFNOSUPPORT ? 97 : errno;
        printf("%d %d ", result, error);
        for (size_t i = 0; i != sizeof(output); ++i) printf("%02X", output[i]);
        putchar('\n');
    }
    return 0;
}
