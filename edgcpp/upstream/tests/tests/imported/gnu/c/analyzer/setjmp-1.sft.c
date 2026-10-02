//type: fp
//options: 
# 0 "./analyzer/setjmp-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/setjmp-1.c"

# 1 "./analyzer/../pr26983.c" 1






void *jmpbuf[6];

void
foo (void)
{
  __builtin_setjmp (jmpbuf);
}

int
main (void)
{
  return 0;
}
# 3 "./analyzer/setjmp-1.c" 2
