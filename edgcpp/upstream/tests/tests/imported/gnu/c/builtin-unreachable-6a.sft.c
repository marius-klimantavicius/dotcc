//type: fp
//options: 
# 0 "./builtin-unreachable-6a.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-unreachable-6a.c"



# 1 "./builtin-unreachable-6.c" 1




void
foo (int b, int c)
{
  void *x = &&lab;
  if (b)
    {
lab:
      __builtin_unreachable ();
    }
lab2:
  if (c)
    x = &&lab2;
  goto *x;
}
# 5 "./builtin-unreachable-6a.c" 2
