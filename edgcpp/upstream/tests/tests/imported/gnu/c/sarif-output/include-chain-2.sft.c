//type: fp
//options: 
# 0 "./sarif-output/include-chain-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./sarif-output/include-chain-2.c"
# 28 "./sarif-output/include-chain-2.c"
# 1 "./sarif-output/include-chain-2.h" 1


void test (void *ptr)
{
  __builtin_free (ptr);
  __builtin_free (ptr);
}
# 29 "./sarif-output/include-chain-2.c" 2
