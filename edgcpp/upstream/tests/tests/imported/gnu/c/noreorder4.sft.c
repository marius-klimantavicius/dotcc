//type: fp
//options: 
# 0 "./noreorder4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./noreorder4.c"



# 1 "./noreorder.c" 1






extern void f2(int);
static int func2(void);





asm("firstasm");

 __attribute__((noipa)) int bozo(void)
{
  f2(3);
  func2();
}

asm("jukjuk");

 __attribute__((noipa)) static int func1(void)
{
  f2(1);
}

asm("barbar");

 __attribute__((noipa)) static int func2(void)
{
  func1();
}

asm("lastasm");
# 5 "./noreorder4.c" 2
