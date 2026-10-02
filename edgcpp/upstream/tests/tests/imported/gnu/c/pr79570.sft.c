//type: fp
//options: 
# 0 "./pr79570.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr79570.c"





# 1 "./pr69956.c" 1




void
fn1 (char *b, char *d, int *c, int i)
{
  for (; i; i++, d++)
    if (b[i])
      *d = c[i];
}
# 7 "./pr79570.c" 2
