//type: fp
//options:  --c++20 --modules
# 0 "./modules/lambda-3_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/lambda-3_b.C"


# 1 "./modules/lambda-3.h" 1

template<int I> inline constexpr auto tmpl = [] {return I;};

inline const auto tpl_1 = tmpl<1>;
inline const auto tpl_2 = tmpl<2>;
# 4 "./modules/lambda-3_b.C" 2
import "lambda-3_a.H";
