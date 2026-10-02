//remark:Nested generic lambdas
//options:--c++17 -tused;fp

  struct S { static constexpr int b = 0; };
  template<typename F> void g(F &&f) { f(S{}); }
  int main() {
    int r = 0;
    g([&](auto id1) {
      g([&](auto id2) { [&]{ r = 1; }(); });  // Previously triggered an
    });                                       // internal error.  Now okay.
    return r;
  }
