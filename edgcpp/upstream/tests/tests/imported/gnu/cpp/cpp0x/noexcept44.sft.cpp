//type: fp
//options: --c++11
// PR c++/90490
// { dg-do compile { target c++11 } }

template <typename>
struct S {
  void g() noexcept(decltype(int{ }) { }) {
  }
};
