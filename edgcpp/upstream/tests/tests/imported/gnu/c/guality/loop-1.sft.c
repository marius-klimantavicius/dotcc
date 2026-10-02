//type: rp
//options: 
# 0 "./guality/loop-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/loop-1.c"



# 1 "./guality/../nop.h" 1
# 5 "./guality/loop-1.c" 2

void __attribute__((noipa,noinline))
foo (int n)
{
  if (n == 0)
    return;
  int i = 0;
  do
    {
      ++i;
    }
  while (i < n);



  __asm__ volatile ("nop" : : "g" (i) : "memory");
}
int main() { foo(1); }
