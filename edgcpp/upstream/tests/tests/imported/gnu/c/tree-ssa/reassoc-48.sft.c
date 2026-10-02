//type: fp
//options: 
# 0 "./tree-ssa/reassoc-48.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/reassoc-48.c"




# 1 "./tree-ssa/reassoc-46.h" 1

unsigned int arr1[1024];
unsigned int arr2[1024];
volatile unsigned int sink;

unsigned int
test (void)
{
  unsigned int sum = 0;
  for (int i = 0; i < 1024; i++)
    {
# 21 "./tree-ssa/reassoc-46.h"
      sink = sum;







      sum += arr1[i];
      sum += arr2[((i ^ 1) + 1) % 1024];
    }
  return sum;
}
# 6 "./tree-ssa/reassoc-48.c" 2
