//remark:ADL and SFINAE
//options:--c++17;fp:--c++17 --microsoft_v=1920;fp

  template<typename> void f();
  template<typename T>
    void g(T p) noexcept(noexcept(void(f<0>(p))));
  namespace N {
    template<typename> struct S {};
    template<int I, typename T> void f(S<T>&);
  }
  int main() {
    g(N::S<int>{});  // Previously failed.  Now okay.
  }
