//remark:Microsoft overload resolution
//options:--microsoft_v=1920;fp:--c++17;fn

struct R {};
struct S {
  S(S&);
  operator const R() const;
};
struct C {
  C(S);
  C(R const&);
};

extern S const sc;
C c(sc);

