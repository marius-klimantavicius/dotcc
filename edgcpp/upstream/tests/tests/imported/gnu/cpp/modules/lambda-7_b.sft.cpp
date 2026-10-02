//type: fp
//options:  --c++20 --modules
# 0 "./modules/lambda-7_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/lambda-7_b.C"




# 1 "./modules/lambda-7.h" 1
struct S {
  int (*a)(int) = [](int x) { return x * 2; };

  int b(int x, int (*f)(int) = [](int x) { return x * 3; }) {
    return f(x);
  }

  static int c(int x, int (*f)(int) = [](int x) { return x * 4; }) {
    return f(x);
  }
};
# 6 "./modules/lambda-7_b.C" 2
import "lambda-7_a.H";
