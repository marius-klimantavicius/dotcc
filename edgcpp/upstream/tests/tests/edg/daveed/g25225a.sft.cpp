//remark:Microsoft overload resolution
//options:--microsoft_v=1920;fp:--c++17;fn

  struct R {};
  struct S {
    S(S&);
    operator const R() const;
  };
  int  g(S);         // (1)
  int  g(R const&);  // (2)
  extern S const sc;
  int r = g(sc);  // Normally an error.  Now okay in Microsoft C++ modes.

