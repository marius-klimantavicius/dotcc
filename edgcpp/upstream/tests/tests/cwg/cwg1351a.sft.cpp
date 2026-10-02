//type:rp
//options_all:--c++17 -tused -A
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
