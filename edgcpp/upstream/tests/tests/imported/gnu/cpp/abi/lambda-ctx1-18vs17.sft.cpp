//type: fp
//options: --c++20
# 0 "./abi/lambda-ctx1-18vs17.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/lambda-ctx1-18vs17.C"



# 1 "./abi/lambda-ctx1.h" 1
inline auto L2 = [] <typename T, typename U> (T, U) -> void {};
namespace B
{
  inline auto L3 = [] <typename T, typename U> (T, U) -> void {};
}

struct C
{
  int f = [] (auto){ return 1;}(&C::f);
  C ();
};

C::C ()
{
  L2 (1, 1.2f);
  B::L3 (1u, 1.2);
}

template <typename A, typename B> int foo (A&&, B&&) {return 0;}
inline int q = foo ([](){}, [](){});
# 5 "./abi/lambda-ctx1-18vs17.C" 2
