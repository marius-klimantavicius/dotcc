//type: cp
//options:  -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w -w
# 0 "./tree-ssa/ifc-20040816-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/ifc-20040816-1.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./tree-ssa/ifc-20040816-1.c" 2





# 9 "./tree-ssa/ifc-20040816-1.c"
int A[16] = {36,39,42,45,43,32,21,12,23,34,45,56,67,78,89,11};

extern void abort(void);

int main1 ()
{
  int i, j;

  for (i = 0; i < 16; i++)
    {
      j = A[i];
      A[i] = ( j >= 42 ? 42 : 0);
    }


  for (i = 0; i < 16; i++)
    {
      if (A[i] > 42)
 abort ();
    }

  return 0;
}
