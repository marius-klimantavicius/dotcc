//type: fp
//options: 
# 0 "./builtin-dynamic-object-size-18.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-dynamic-object-size-18.c"







# 1 "./builtin-object-size-18.c" 1






typedef long unsigned int size_t;

size_t
foo (const char *p, size_t s, size_t t)
{
  char buf[64];
  char *q = __builtin___stpncpy_chk (buf, p, s, t);
  return __builtin_dynamic_object_size (q, 2);
}
# 9 "./builtin-dynamic-object-size-18.c" 2
