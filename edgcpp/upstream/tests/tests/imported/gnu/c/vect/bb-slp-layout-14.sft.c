//type: fp
//options: 
# 0 "./vect/bb-slp-layout-14.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/bb-slp-layout-14.c"



# 1 "./vect/bb-slp-layout-13.c" 1


int a[4], b[4], c[4], d[4];

void f1()
{
  a[0] = (b[1] << c[3]) - (d[1] >> c[3]);
  a[1] = (b[0] << c[2]) - (d[0] >> c[2]);
  a[2] = (b[3] << c[1]) - (d[3] >> c[1]);
  a[3] = (b[2] << c[0]) - (d[2] >> c[0]);
}
# 5 "./vect/bb-slp-layout-14.c" 2
