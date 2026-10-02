//type: fp
//options: 
# 0 "./vect/bb-slp-layout-10.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/bb-slp-layout-10.c"



# 1 "./vect/bb-slp-layout-9.c" 1



int a[4], b[400], c[400], d[40000];

void f1()
{
  int a0 = a[0];
  int a1 = a[1];
  int a2 = a[2];
  int a3 = a[3];
  for (int i = 0; i < 100; ++i)
    {
      a0 ^= c[i * 4 + 0];
      a1 ^= c[i * 4 + 1];
      a2 ^= c[i * 4 + 2];
      a3 ^= c[i * 4 + 3];
      for (int j = 0; j < 100; ++j)
 {
   a0 += d[i * 400 + j * 4 + 1];
   a1 += d[i * 400 + j * 4 + 0];
   a2 += d[i * 400 + j * 4 + 3];
   a3 += d[i * 400 + j * 4 + 2];
 }
      b[i * 4 + 0] = a0;
      b[i * 4 + 1] = a1;
      b[i * 4 + 2] = a2;
      b[i * 4 + 3] = a3;
    }
  a[0] = a0;
  a[1] = a1;
  a[2] = a2;
  a[3] = a3;
}
# 5 "./vect/bb-slp-layout-10.c" 2
