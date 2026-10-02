//type: rp
//options: 
# 0 "./gcov/gcov-3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./gcov/gcov-3.C"






# 1 "./gcov/gcov-3.h" 1
# 21 "./gcov/gcov-3.h"
struct T {
  int i;
  T() { i = 0; }
};

T t;

int foo()
{
  return t.i;
}
# 8 "./gcov/gcov-3.C" 2

extern int foo();

int
main ()
{
  return foo();
}
