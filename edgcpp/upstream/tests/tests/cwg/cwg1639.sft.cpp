//type:rp
//options_all:--c++14 -tused -A
  template<class T> struct A {
    void f() noexcept(false) {}
    void g() noexcept(true) {}
  };

  int main() {
    if (noexcept((A<short>().*(&A<short>::f))()))
      return 1;

    if (!noexcept((A<long>().*(&A<long>::g))()))
      return 1;

     return 0;
  }

//cwg: 1639
//title: exception-specifications and pointer/pointer-to-member expressions (Resolved by 1351)
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
