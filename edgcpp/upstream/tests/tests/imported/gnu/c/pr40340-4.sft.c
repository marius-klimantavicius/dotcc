//type: fp
//options: 
# 0 "./pr40340-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr40340-4.c"





# 1 "./pr40340.h" 1
       
# 2 "./pr40340.h" 3

# 2 "./pr40340.h" 3
typedef long unsigned int size_t;
extern void *memset (void *s, int c, size_t n)
  __attribute__ ((nothrow, nonnull (1)));
extern inline
__attribute__ ((always_inline, artificial, gnu_inline, nothrow))
void *
memset (void *dest, int ch, size_t len)
{
  return __builtin___memset_chk (dest, ch, len,
     __builtin_object_size (dest, 0));
}
# 25 "./pr40340.h" 3
static inline void
__attribute__ ((always_inline))
test3 (char *p)
{
  memset (p, 0, 6);
}
# 7 "./pr40340-4.c" 2


# 8 "./pr40340-4.c"
int
main (void)
{
  char buf[4];
  test3 (buf);
  return 0;
}
