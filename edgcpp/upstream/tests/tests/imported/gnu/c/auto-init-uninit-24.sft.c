//type: fp
//options: 
# 0 "./auto-init-uninit-24.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-24.c"


# 1 "./uninit-24.c" 1



int foo (int x)
{
  int y;
  if (x)
    return *(&y + 1);
  return 0;
}
# 4 "./auto-init-uninit-24.c" 2
