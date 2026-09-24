#define _DEFAULT_SOURCE 1
#include <grp.h>
#include <strings.h>
#include <termios.h>
#include <libgen.h>
#include <stddef.h>
#include <stdio.h>

int main(void) {
    struct group entry = {0};
    struct termios terminal = {0};
    entry.gr_gid = 37;
    terminal.c_cc[VMIN] = 3;
    terminal.c_lflag = ECHO | ICANON;
    printf("%lu %lu %lu %u\n", (unsigned long)sizeof(entry),
           (unsigned long)offsetof(struct group, gr_gid),
           (unsigned long)offsetof(struct group, gr_mem), entry.gr_gid);
    printf("%lu %lu %lu %u %u\n", (unsigned long)sizeof(terminal),
           (unsigned long)offsetof(struct termios, c_cc),
           (unsigned long)offsetof(struct termios, c_ispeed),
           (unsigned)terminal.c_cc[VMIN], (unsigned)terminal.c_lflag);
    printf("%d %d %d %d\n", strcasecmp("VaLkEy", "valkey") == 0,
           strncasecmp("Prefix-left", "PREFIX-right", 6) == 0,
           strncasecmp("x", "Y", 0) == 0,
           strcasecmp("\200", "\177") > 0);
    return 0;
}
