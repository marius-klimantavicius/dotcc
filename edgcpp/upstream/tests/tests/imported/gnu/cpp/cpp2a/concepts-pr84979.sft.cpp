//type: s
//options: --c++11
// { dg-do compile { target c++11 } }
// { dg-options "-fconcepts" }

template<typename> void foo() {}

void bar()
{
  foo<auto>(); // { dg-error "" }
}
