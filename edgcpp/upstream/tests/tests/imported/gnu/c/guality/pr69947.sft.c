//type: rp
//options: 
# 0 "./guality/pr69947.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr69947.c"




# 1 "./guality/../nop.h" 1
# 6 "./guality/pr69947.c" 2

static const char *c = "foobar";

__attribute__((noinline, noclone)) void
foo (void)
{
  static const char a[] = "abcdefg";
  const char *b = a;
  asm ("nop" : : : "memory");
}

int
main ()
{
  foo ();
  return 0;
}
