//type: fp
//options: 
# 0 "./pr87600-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr87600-1.c"




# 1 "./pr87600.h" 1
# 6 "./pr87600-1.c" 2



long
test0 (long arg)
{
  register long var asm ("rax");
  asm ("blah %0 %1" : "+&r" (var) : "r" (arg));
  return var;
}

long
test1 (long arg0, long arg1)
{
  register long var asm ("rax");
  asm ("blah %0, %1, %2" : "=&r" (var) : "r" (arg0), "0" (arg1));
  return var + arg1;
}

long
test2 (void)
{
  register long var1 asm ("rax");
  register long var2 asm ("rax");
  asm ("blah %0 %1" : "=&r" (var1) : "0" (var2));
  return var1;
}

long
test3 (void)
{
  register long var1 asm ("rax");
  register long var2 asm ("rdx");
  long var3;
  asm ("blah %0 %1" : "=&r" (var1), "=r" (var3) : "1" (var2));
  return var1 + var3;
}

long
test4 (void)
{
  register long var1 asm ("rax");
  register long var2 asm ("rdx");
  register long var3 asm ("rdx");
  asm ("blah %0 %1" : "=&r" (var1), "=r" (var2) : "1" (var3));
  return var1;
}
