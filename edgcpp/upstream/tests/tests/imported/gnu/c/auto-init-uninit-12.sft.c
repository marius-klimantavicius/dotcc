//type: fp
//options: 
# 0 "./auto-init-uninit-12.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-12.c"



# 1 "./uninit-12.c" 1




typedef _Complex float C;
C foo()
{
  C f;
  __real__ f = 0;
  __imag__ f = 0;
  return f;
}
# 5 "./auto-init-uninit-12.c" 2
