//type: fp
//options: 
# 0 "./tree-ssa/ssa-dse-38.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/ssa-dse-38.c"






# 1 "./tree-ssa/ssa-dse-37.c" 1







extern void frob (char *);

void g (char *s)
{
  extern char a[8];
  __builtin_strncpy (a, s, sizeof a);
  __builtin_memset (a, 0, sizeof a);
  frob (a);
}

void h (char *s)
{
  extern char a[8];
  __builtin_memset (a, 0, sizeof a);
  __builtin_strncpy (a, s, sizeof a);
  frob (a);
}

void i (char *s)
{
  extern char a[8];
  __builtin_strncpy (a, s, sizeof a);
  __builtin_memset (a, 0, sizeof a - 5);
  frob (a);
}

void j (char *s)
{
  extern char a[8];
  __builtin_memset (a, 0, sizeof a);
  __builtin_strncpy (a, s, sizeof a - 5);
  frob (a);
}

void l (char *s)
{
  extern char a[8];
  __builtin_strncpy (a, s, sizeof a);
  __builtin_memset (a + 2, 0, sizeof a - 2);
  frob (a);
}

void m (char *s)
{
  extern char a[8];
  __builtin_memset (a, 0, sizeof a);
  __builtin_strncpy (a + 2, s, sizeof a - 2);
  frob (a);
}
# 8 "./tree-ssa/ssa-dse-38.c" 2
