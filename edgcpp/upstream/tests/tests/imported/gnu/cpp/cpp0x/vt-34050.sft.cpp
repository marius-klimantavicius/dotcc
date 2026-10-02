//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }
struct A {};

template<typename... T> struct B : T...
{
  B() : T()... {}
};

B<A> b;
