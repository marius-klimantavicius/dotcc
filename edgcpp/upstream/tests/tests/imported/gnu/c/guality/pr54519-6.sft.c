//type: rp
//options: 
# 0 "./guality/pr54519-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr54519-6.c"




# 1 "./guality/../nop.h" 1
# 6 "./guality/pr54519-6.c" 2

static inline void
f1 (int x, int y)
{
  asm volatile ("nop");
  asm volatile ("nop");
}

static inline void
f2 (int z)
{
  f1 (z, 0);
  f1 (z, 1);
}

int
main ()
{
  f2 (2);
  f2 (3);
  return 0;
}
