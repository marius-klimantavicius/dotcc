//source_files: pr56154-aux.c
//type: rp
//options: 
# 0 "./guality/pr56154-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr56154-1.c"





# 1 "./guality/../nop.h" 1
# 7 "./guality/pr56154-1.c" 2

union U { int a, b; };
volatile int z;

__attribute__((noinline, noclone)) int
foo (int fd, union U x)
{
  int result = x.a != 0;
  if (fd != 0)
    result = x.a == 0;
  asm ("nop" : : : "memory");
  z = x.a;
  x.a = 6;
  asm ("nop" : : : "memory");
  return result;
}

void
test_main (void)
{
  union U u = { .a = 4 };
  foo (0, u);
}
