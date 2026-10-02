//type: fp
//options: 
# 0 "./lto/20100302_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20100302_1.C"


# 1 "./lto/20100302.h" 1
typedef float mm128 __attribute ((vector_size (16)));

template <class T>
struct A
{
  static T t;
};

void f (mm128 *);
# 4 "./lto/20100302_1.C" 2

int main()
{
  f(& A<mm128>::t);
}
