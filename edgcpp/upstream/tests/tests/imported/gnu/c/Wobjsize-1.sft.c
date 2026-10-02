//type: fp
//options: 
# 0 "./Wobjsize-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wobjsize-1.c"



# 1 "./Wobjsize-1.h" 1
       
# 2 "./Wobjsize-1.h" 3


# 3 "./Wobjsize-1.h" 3
extern __inline __attribute__ ((__always_inline__,__artificial__,__gnu_inline__)) char *
__attribute__ ((__nothrow__)) strcpy (char *__restrict __dest, __const char *__restrict __src)
{
  return __builtin___strcpy_chk (__dest, __src, __builtin_object_size (__dest, 2 > 1));
}
# 5 "./Wobjsize-1.c" 2


# 6 "./Wobjsize-1.c"
char buf[6];

int main(int argc, char **argv)
{
  strcpy (buf,"hello ");
  return 0;
}
