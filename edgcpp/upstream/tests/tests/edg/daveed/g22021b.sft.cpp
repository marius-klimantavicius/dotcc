//remark:Substitution and nontype template arguments
//options:--c++17;fn

  template<typename T> struct W { using Type = T; };
  template<typename, int N = 42> struct X {
    static constexpr int num = N;
    template <int = num> constexpr X() {}
  };
  template<typename C> X(C &) -> X<typename C::Type>;
  void g(W<char> x) {
    X{ x };
  }
