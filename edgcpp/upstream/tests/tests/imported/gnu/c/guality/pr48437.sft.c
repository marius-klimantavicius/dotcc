//type: rp
//options: 
# 0 "./guality/pr48437.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr48437.c"




# 1 "./guality/../nop.h" 1
# 6 "./guality/pr48437.c" 2

int i __attribute__((used));
int main()
{
  volatile int i;
  for (i = 3; i < 7; ++i)
    {
      extern int i;
      asm volatile ("nop" : : : "memory");
    }
  return 0;
}
