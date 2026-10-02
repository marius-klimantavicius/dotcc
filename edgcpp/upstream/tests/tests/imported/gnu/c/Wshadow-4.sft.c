//type: fp
//options: 
# 0 "./Wshadow-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wshadow-4.c"



# 1 "./Wshadow-4.h" 1
       
# 2 "./Wshadow-4.h" 3

# 2 "./Wshadow-4.h" 3
extern void index1 (void);
# 5 "./Wshadow-4.c" 2

# 5 "./Wshadow-4.c"
extern void index2 (void);

void foo (double index1,
   double index2)
{
}

void foo1 (void)
{
  double index1;
  double index2;
}

void foo2 (void)
{
  {
    double index1;
    double index2;
  }
}

void foo3 (void)
{
  void (*index1)(void);
  void (*index2)(void);
}

void foo4 (void)
{
  void index1(void) {};
  void index2(void) {};
}
