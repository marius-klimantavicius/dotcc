//type: fp
//options: 
# 0 "./auto-init-uninit-18.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-18.c"


# 1 "./uninit-18.c" 1



char *foo(int bar, char *baz)
{
  char *tmp;

  if (bar & 3)
    tmp = baz;

  switch (bar) {
  case 1:
    tmp[5] = 7;
    break;
  case 2:
    tmp[11] = 15;
    break;
  default:
    tmp = 0;
    break;
  }

  return tmp;
}
# 4 "./auto-init-uninit-18.c" 2
