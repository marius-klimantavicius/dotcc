//type: fp
//options: 
# 0 "./tree-ssa/pr89487.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr89487.c"




# 1 "./tree-ssa/../pr87600.h" 1
# 6 "./tree-ssa/pr89487.c" 2

void
caml_interprete (void)
{

  register int *pc asm("rax");
  register int *sp asm("rdx");
  int i;

  for (i = 0; i < 3; ++i)
    *--sp = pc[i];

}
