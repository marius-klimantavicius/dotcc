//type: fn
//options: 
# 0 "./pr87600-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr87600-2.c"




# 1 "./pr87600.h" 1
# 6 "./pr87600-2.c" 2



long
test0 (void)
{
  register long var1 asm ("rax");
  register long var2 asm ("rax");
  asm ("blah %0 %1" : "=r" (var1), "=r" (var2));
  return var1;
}

long
test1 (void)
{
  register long var1 asm ("rax");
  register long var2 asm ("rdx");
  asm ("blah %0 %1" : "=r" (var1) : "0" (var2));
  return var1;
}

long
test2 (void)
{
  register long var1 asm ("rax");
  register long var2 asm ("rax");
  asm ("blah %0 %1" : "=&r" (var1) : "r" (var2));
  return var1;
}

long
test3 (void)
{
  register long var1 asm ("rax");
  register long var2 asm ("rax");
  long var3;
  asm ("blah %0 %1" : "=&r" (var1), "=r" (var3) : "1" (var2));
  return var1 + var3;
}
