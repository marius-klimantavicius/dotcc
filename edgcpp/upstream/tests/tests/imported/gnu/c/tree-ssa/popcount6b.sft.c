//type: fp
//options: 
# 0 "./tree-ssa/popcount6b.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/popcount6b.c"



# 1 "./tree-ssa/popcount6.c" 1



int g(int n)
{
  n &= 0x8000;
  if (n == 0)
    return 1;
  return __builtin_popcount(n);
}
# 5 "./tree-ssa/popcount6b.c" 2
