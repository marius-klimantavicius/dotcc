//type: fp
//options:  -w
# 0 "./pr81621.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr81621.c"




# 1 "./graphite/scop-10.c" 1
void bar (void);

int toto()
{
  int i, j, k;
  int a[100][100];
  int b[200];

  for (i = 1; i < 100; i++)
    {
      for (j = 1; j < 100; j++)
        b[i+j] = b[i+j-1] + 2;

      if (i * 2 == i + 8)
        {
   for (j = 1; j < 100; j++)
     b[i+j] = b[i+j-1] + 2;
   a[i][i] = 2;
 }

      for (k = 1; k < 100; k++)
        b[i+k] = b[i+k-5] + 2;
    }

  return a[3][5] + b[2];
}
# 6 "./pr81621.c" 2
