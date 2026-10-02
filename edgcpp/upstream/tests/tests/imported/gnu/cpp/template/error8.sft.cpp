//type: fn
//options:  --c++17 --c++14
// PR c++/11116

template <typename T> struct S {};

void f() {
  throw S (); // { dg-error "12:class template argument deduction failed|no match" "" { target c++17 } }
  // { dg-error "11:missing template arguments" "" { target c++14_down } .-1 }
}
