//type: fp
//options: 
# 0 "./auto-init-uninit-C.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-C.c"




# 1 "./uninit-C.c" 1






typedef int TItype __attribute__ ((mode (TI)));





TItype
__subvdi3 (TItype a, TItype b)
{
  TItype w;

  w = a - b;

  return w;
}
# 6 "./auto-init-uninit-C.c" 2
