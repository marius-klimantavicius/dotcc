//type: fp
//options: 
# 0 "./vect/ggc-pr37574.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/ggc-pr37574.c"


# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./vect/ggc-pr37574.c" 2


# 5 "./vect/ggc-pr37574.c"
unsigned short in[40 +128];
int main (void) {
  int i = 0, j = 0;
  unsigned int diff;
  unsigned int s=0,sum=0;
  for (i = 0; i < 40;i++)
   {
     diff = 0;
     for (j = 0; j < 128;j+=8)
       diff += in[j+i];
     s += ((unsigned short)diff>>3);
   }
   if (s != sum)
     return -1;
   return 0;
}
