//type: fp
//options:  --c++20 --c++20 --modules
# 0 "./modules/nested-constr-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/nested-constr-1_b.C"


# 1 "./modules/nested-constr-1.h" 1

template<typename T>
struct traits
{
  template<typename U>
    struct nested
    { using type = void; };

  template<typename U> requires requires { typename U::type; }
    struct nested<U>
    { using type = typename U::type; };
};

using V = traits<char>::nested<int>::type;
# 4 "./modules/nested-constr-1_b.C" 2
import "nested-constr-1_a.H";

struct X
{
  using type = int;
};

traits<char>::nested<X>::type b;
