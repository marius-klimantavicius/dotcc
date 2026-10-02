//type: fp
//options: --c++11
// PR c++/99893
// { dg-do compile { target c++11 } }

void f(...);

template<class... Ts>
void g() {
  f([] { static_assert(Ts::value, ""); }...);
}
