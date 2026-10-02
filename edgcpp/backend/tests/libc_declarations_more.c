#include <ctype.h>
#include <string.h>
#include <time.h>
#include <stddef.h>
#ifdef __cplusplus
#define ASSERT static_assert
#else
#define ASSERT _Static_assert
#endif
ASSERT(sizeof(time_t)==8 && sizeof(clock_t)==8,"time scalar ABI");
ASSERT(sizeof(struct tm)==56 && offsetof(struct tm,tm_gmtoff)==40 && offsetof(struct tm,tm_zone)==48,"broken-down time ABI");
ASSERT(sizeof(struct timespec)==16 && offsetof(struct timespec,tv_nsec)==8,"timespec ABI");
ASSERT(sizeof(&isalnum)==sizeof(void*) && sizeof(&isalpha)==sizeof(void*) && sizeof(&isblank)==sizeof(void*) && sizeof(&iscntrl)==sizeof(void*),"ctype declarations 1");
ASSERT(sizeof(&isdigit)==sizeof(void*) && sizeof(&isgraph)==sizeof(void*) && sizeof(&islower)==sizeof(void*) && sizeof(&isprint)==sizeof(void*),"ctype declarations 2");
ASSERT(sizeof(&ispunct)==sizeof(void*) && sizeof(&isspace)==sizeof(void*) && sizeof(&isupper)==sizeof(void*) && sizeof(&isxdigit)==sizeof(void*) && sizeof(&tolower)==sizeof(void*) && sizeof(&toupper)==sizeof(void*),"ctype declarations 3");
ASSERT(sizeof(&strcoll)==sizeof(void*) && sizeof(&strxfrm)==sizeof(void*) && sizeof(&strerror)==sizeof(void*),"string declarations");
ASSERT(sizeof(&clock)==sizeof(void*) && sizeof(&difftime)==sizeof(void*) && sizeof(&mktime)==sizeof(void*) && sizeof(&time)==sizeof(void*),"time declarations 1");
ASSERT(sizeof(&asctime)==sizeof(void*) && sizeof(&ctime)==sizeof(void*) && sizeof(&gmtime)==sizeof(void*) && sizeof(&localtime)==sizeof(void*) && sizeof(&strftime)==sizeof(void*) && sizeof(&timespec_get)==sizeof(void*),"time declarations 2");
int main(void) {struct tm value={0};struct timespec stamp={0};value.tm_year=126;stamp.tv_sec=42;return value.tm_year!=126 || stamp.tv_sec!=42;}
