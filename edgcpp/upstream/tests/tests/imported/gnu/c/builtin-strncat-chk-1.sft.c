//type: fp
//options: --c99 --strict_gnu
# 0 "./builtin-strncat-chk-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-strncat-chk-1.c"





extern void abort (void);

# 1 "./../gcc.c-torture/execute/builtins/chk.h" 1
# 78 "./../gcc.c-torture/execute/builtins/chk.h"
extern void *chk_fail_buf[];
extern volatile int chk_fail_allowed, chk_calls;
extern volatile int memcpy_disallowed, mempcpy_disallowed, memmove_disallowed;
extern volatile int memset_disallowed, strcpy_disallowed, stpcpy_disallowed;
extern volatile int strncpy_disallowed, stpncpy_disallowed, strcat_disallowed;
extern volatile int strncat_disallowed, sprintf_disallowed, vsprintf_disallowed;
extern volatile int snprintf_disallowed, vsnprintf_disallowed;
# 9 "./builtin-strncat-chk-1.c" 2

char buf1[20];
char *q;

void
test (int arg, ...)
{
  char *p = &buf1[10];

  *p = 0;
  __builtin___strncat_chk (p, "abcdefg", 9, __builtin_object_size (p, 0));
  *p = 0;
  __builtin___strncat_chk (p, "abcdefghi", 9, __builtin_object_size (p, 0));
  *p = 0;
  __builtin___strncat_chk (p, "abcdefghij", 9, __builtin_object_size (p, 0));
  *p = 0;
  __builtin___strncat_chk (p, "abcdefghi", 10, __builtin_object_size (p, 0));
  *p = 0;
  __builtin___strncat_chk (p, "abcdefghij", 10, __builtin_object_size (p, 0));
  *p = 0;
  __builtin___strncat_chk (p, "abcdefgh", 11, __builtin_object_size (p, 0));
  *p = 0;
  __builtin___strncat_chk (p, "abcdefghijkl", 11, __builtin_object_size (p, 0));
  *p = 0;
  __builtin___strncat_chk (p, q, 9, __builtin_object_size (p, 0));
  *p = 0;
  __builtin___strncat_chk (p, q, 10, __builtin_object_size (p, 0));
  *p = 0;
  __builtin___strncat_chk (p, q, 11, __builtin_object_size (p, 0));
}
