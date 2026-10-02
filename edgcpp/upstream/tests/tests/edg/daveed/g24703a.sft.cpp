//remark:Equality ambiguity tie-breaker
//options:--c++20;fn:--c++20 --clang;fn:--c++20 --gnu=100200;fp

  struct S {
    S();
    operator int() const;
  } s;
  bool operator==(S const&, char const*);  // (1)
  bool operator==(char const*, S const&);  // (2)
  auto r = (s == 0);  // Previously ambiguous in C++20 modes.  Now accepted
                      // in GNU C++20 mode.
