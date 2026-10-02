//type: fp
//options: 
# 0 "./analyzer/pr61861.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr61861.c"


# 1 "./analyzer/../../gcc.dg/pr61861.c" 1



extern void foo (int);
extern void bar (int, char *);
# 17 "./analyzer/../../gcc.dg/pr61861.c"
void
f (void)
{
  foo ("./analyzer/../../gcc.dg/pr61861.c");
  foo ("./analyzer/pr61861.c");
  foo ("07:43:48");
  foo ("Feb 28 2025");
  foo ("Sun Feb 23 17:32:21 2025");
  bar (1, 25);
  bar (0, 1);

  foo ("./analyzer/../../gcc.dg/pr61861.c");
  foo ("07:43:48");
  foo ("Feb 28 2025");
  bar (1, 31);

  foo ("foo");
  foo ("foo");
  foo ("foo");
  bar (1, 42);
}
# 4 "./analyzer/pr61861.c" 2
