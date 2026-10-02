//type: fp
//options: --c99 --strict_gnu
# 0 "./format/attr-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/attr-5.c"
# 10 "./format/attr-5.c"
static int scanf(const char *restrict, ...);



extern int sscanf(const char *restrict, const char *restrict, int *);

void
foo (const char *s, int *p)
{
  scanf("%ld", p);
  sscanf(s, "%ld", p);
}


static int
scanf (const char *restrict fmt, ...)
{
  return 0;
}
