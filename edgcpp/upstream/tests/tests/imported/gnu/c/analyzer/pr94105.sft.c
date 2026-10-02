//type: fp
//options: 
# 0 "./analyzer/pr94105.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr94105.c"


# 1 "./analyzer/../../c-c++-common/torture/pr58794-1.c" 1


struct S0
{
  int f;
};

struct S1
{
  struct S0 f1;
  volatile int f2;
};

struct S2
{
  struct S1 g;
} a, b;

static int *c[1][2] = {{0, (int *)&a.g.f2}};
static int d;

int
main ()
{
  for (d = 0; d < 1; d++)
    for (b.g.f1.f = 0; b.g.f1.f < 1; b.g.f1.f++)
      *c[b.g.f1.f][d + 1] = 0;
  return 0;
}
# 4 "./analyzer/pr94105.c" 2
