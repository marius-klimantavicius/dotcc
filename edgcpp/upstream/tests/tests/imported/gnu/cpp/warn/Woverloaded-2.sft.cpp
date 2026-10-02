//type: fp
//options: 
# 0 "./warn/Woverloaded-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Woverloaded-2.C"



# 1 "./warn/Woverloaded-2.h" 1
       
# 2 "./warn/Woverloaded-2.h" 3


# 3 "./warn/Woverloaded-2.h" 3
struct A
{
  virtual void f();
};
# 5 "./warn/Woverloaded-2.C" 2


# 6 "./warn/Woverloaded-2.C"
struct B : A
{
  void f(int);
};
