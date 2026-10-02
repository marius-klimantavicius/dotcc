//remark:Dependent destructors in constant expressions
//options:--gnu=80300;fp

  template<bool> struct X {};
  template<typename T> X<(T().~T())> g();
