//type: fp
//options: 
# 0 "./analyzer/torture/20060625-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/torture/20060625-1.c"
# 1 "./analyzer/torture/../../../gcc.c-torture/compile/20060625-1.c" 1



_Complex float b;

void foo (void)
{
  _Complex float a = 3.40282346638528859811704183484516925e+38F;
  b = 3.40282346638528859811704183484516925e+38F + a;
}
# 2 "./analyzer/torture/20060625-1.c" 2
