//type: lp
//options: 
# 0 "./tree-ssa/vrp69.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/vrp69.c"



# 1 "./tree-ssa/vrp.h" 1
extern void link_error(void);
# 5 "./tree-ssa/vrp69.c" 2

void test1(int i, int j)
{
  if (i < 1 || i > 5) return;;
  if (j < 7 || j > 10) return;;
  do { __typeof__ (i + j) v = (i + j); if (v < 8) link_error(); if (v > 15) link_error(); if (v < 8 || v > 15) link_error (); } while (0);
}


void test2(unsigned int i)
{
  if (i < 2*(unsigned)0x7fffffff + 1 - 0x4 || i > 2*(unsigned)0x7fffffff + 1 - 0x1) return;;
  do { __typeof__ (i + 0x2) v = (i + 0x2); if (v >= 0x1) if (v <= 2*(unsigned)0x7fffffff + 1 - 0x3) link_error(); if (v >= 0x1 && v <= 2*(unsigned)0x7fffffff + 1 - 0x3) link_error(); } while (0);
}
void test3(unsigned int i)
{
  if (i < 2*(unsigned)0x7fffffff + 1 - 0x4 || i > 2*(unsigned)0x7fffffff + 1 - 0x1) return;;
  do { __typeof__ (i + 0x5) v = (i + 0x5); if (v < 0x0) link_error(); if (v > 0x3) link_error(); if (v < 0x0 || v > 0x3) link_error (); } while (0);
}
void test4(unsigned int i)
{
  if (i < 2 || i > 4) return;;
  do { __typeof__ (i - 4) v = (i - 4); if (v >= 1) if (v <= 2*(unsigned)0x7fffffff + 1 - 2) link_error(); if (v >= 1 && v <= 2*(unsigned)0x7fffffff + 1 - 2) link_error(); } while (0);
}
void test5(unsigned int i)
{
  if (i < 2 || i > 4) return;;
  do { __typeof__ (i - 8) v = (i - 8); if (v < 2*(unsigned)0x7fffffff + 1 - 5) link_error(); if (v > 2*(unsigned)0x7fffffff + 1 - 3) link_error(); if (v < 2*(unsigned)0x7fffffff + 1 - 5 || v > 2*(unsigned)0x7fffffff + 1 - 3) link_error (); } while (0);
}

int main() {}
