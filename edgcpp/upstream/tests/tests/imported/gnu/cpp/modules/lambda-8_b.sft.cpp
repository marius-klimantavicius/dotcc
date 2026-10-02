//type: fn
//options:  --c++20 --modules --c++20
# 0 "./modules/lambda-8_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/lambda-8_b.C"



# 1 "./modules/lambda-8.h" 1


template <typename> struct S {
  template <typename> using t = decltype([]{});
};


using t = S<int>::t<int>;
# 5 "./modules/lambda-8_b.C" 2
import "lambda-8_a.H";
