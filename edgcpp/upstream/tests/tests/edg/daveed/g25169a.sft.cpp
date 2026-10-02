//remark:Microsoft template parsing relaxations
//options:--microsoft_v=1928 --c++17;fp:--c++17;fn

template<typename T> struct S {
  void f(S &x) {
    [x](auto) { x.g<int>(T{}); };
  }
  template<typename U, typename = typename U::X>
  void g(U) {}
};

S<int> si;
int main() {
  si.f(si);
}

