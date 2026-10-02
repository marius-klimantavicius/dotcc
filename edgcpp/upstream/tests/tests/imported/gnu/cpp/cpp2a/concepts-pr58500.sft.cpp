//type: s
//options: --c++14
// PR c++/58500
// { dg-do compile { target c++14 } }
// { dg-additional-options "-fconcepts" }

struct A {};

void foo(auto (A::*)());
