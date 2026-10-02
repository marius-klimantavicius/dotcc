//type: lp
//options: 
# 0 "./tree-ssa/vrp68.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/vrp68.c"



# 1 "./tree-ssa/vrp.h" 1
extern void link_error(void);
# 5 "./tree-ssa/vrp68.c" 2

void test1 (int i, int j, int b)
{
  if (i < 2 || i > 6) return;;
  if (j >= 1 && j <= 7) return;;
  if (b) i = j;;
  do { __typeof__ (i) v = (i); if (v >= 1) if (v <= 1) link_error(); if (v >= 1 && v <= 1) link_error(); } while (0);
}
int main() { }
