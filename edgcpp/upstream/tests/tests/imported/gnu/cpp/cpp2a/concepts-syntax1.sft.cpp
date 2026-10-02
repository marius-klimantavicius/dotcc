//type: fn
//options: --c++20
// PR c++/92517
// { dg-do compile { target concepts } }

template <typename T>
concept C = true;

template<int I>
requires C decltype<I>		// { dg-error "" }
void f() {}
