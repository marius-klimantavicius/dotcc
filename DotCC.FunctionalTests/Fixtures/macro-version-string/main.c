#include <stdio.h>
#define MAJOR 1
#define MINOR 5
#define PATCH 7
#define NUMBERS MAJOR.MINOR.PATCH
#define STR(x) #x
#define XSTR(x) STR(x)
#define VERSION XSTR(NUMBERS)
#define SPACED_VERSION XSTR(MAJOR . MINOR . PATCH)
#define NAME identifier
#define PUNCTUATION XSTR(NAME+NAME->member)
#define EMPTY
#define REMOVED XSTR(NAME EMPTY+NAME)
#define LITERAL "a\\b\"c"
#define QUOTED XSTR(NAME/**/+   LITERAL)
#define INNER x EMPTY
#define OUTER XSTR(INNER+y)
#define FORWARD(arg) XSTR(arg+y)
#define PREFIX XSTR(
static const char *version(void) { return VERSION; }
int main(void) {
    puts(version());
    puts(SPACED_VERSION);
    puts(PUNCTUATION);
    puts(REMOVED);
    puts(QUOTED);
    puts(OUTER);
    puts(FORWARD(INNER));
    puts(PREFIX INNER+y));
    puts(XSTR(INNER+y));
    return 0;
}
