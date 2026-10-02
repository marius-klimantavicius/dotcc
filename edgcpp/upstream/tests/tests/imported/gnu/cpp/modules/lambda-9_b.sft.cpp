//type: fp
//options:  --c++20 --modules --c++20
# 0 "./modules/lambda-9_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/lambda-9_b.C"


# 1 "./modules/lambda-9.h" 1

template <typename T>
concept C = requires { []{}; };
# 4 "./modules/lambda-9_b.C" 2
import "lambda-9_a.H";

static_assert(C<int>);
