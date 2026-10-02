//type: fp
//options: 
# 0 "./vect/bb-slp-layout-8.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/bb-slp-layout-8.c"



# 1 "./vect/bb-slp-layout-7.c" 1



int a[4], b[400];

void f1()
{
  for (int i = 0; i < 100; ++i)
    {
      a[0] += b[i * 4 + 3];
      a[1] += b[i * 4 + 2];
      a[2] += b[i * 4 + 1];
      a[3] += b[i * 4 + 0];
    }
}
# 5 "./vect/bb-slp-layout-8.c" 2
