//type: fp
//options: 
# 0 "./tree-ssa/pr88444.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr88444.c"





# 1 "./tree-ssa/../pr88444.c" 1




int v;

int
foo (int, int);

static inline int
bar (long int x)
{
  return !!x ? x : 1;
}

static inline void
baz (int x)
{
  v += foo (0, 0) + bar (x);
}

void
qux (void)
{
  int a = 0;
  v = v || foo (0, 0);
  v = v || foo (0, 0);
  v = v || foo (0, 0);
  baz (a);
}
# 7 "./tree-ssa/pr88444.c" 2
