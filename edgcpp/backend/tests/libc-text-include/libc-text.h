/* Draft declarations to merge into string.h/stdlib.h after the frozen suite. */
#include <stddef.h>
#include <ctype.h>
#include <locale.h>
#ifdef __cplusplus
extern "C" {
#endif
char *strerror(int);
size_t __edg_mb_cur_max(void);
int wctomb(char *, wchar_t);
#ifdef __cplusplus
}
#endif
#define MB_CUR_MAX (__edg_mb_cur_max())
