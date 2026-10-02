//type: fn
//options: --c++11
// PR c++/59270
// { dg-do compile { target c++11 } }

struct A
{
  struct B b; // { dg-error "incomplete type" }
};

decltype(A()) a;
