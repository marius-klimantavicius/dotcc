#include <stdio.h>
#define LETTER 'A'
#define ID(x) x
#define STR(x) #x
#if 'A' == '\301'
#define ENCODING 2
#else
#define ENCODING 1
#endif
# if LETTER == 65 && ID('\101') == 65 && '\x41' == 'A'
#define MACROS 1
#else
#define MACROS 0
#endif
#if '\n' != 10
#define ESCAPES 0
#  elif '\t' == 9 && '\r' == 13 && '\\' == 92 && '\'' == 39 && '\0' == 0
#define ESCAPES 1
#else
#define ESCAPES 0
#endif
int main(void) {
    printf("%d %d %d %d %s\n", ENCODING, MACROS, ESCAPES, LETTER, STR('\101'));
    return 0;
}
