//type: fp
//options: --c99
# 0 "./pr8715.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr8715.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 5 "./pr8715.c" 2

int foo()
{
  unsigned char b = '1';

  
# 10 "./pr8715.c" 3 4
 _Bool 
# 10 "./pr8715.c"
      x = ~b;

  return 0;
}
