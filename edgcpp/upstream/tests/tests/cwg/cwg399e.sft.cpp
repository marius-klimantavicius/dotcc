//type:fn
//options_all:--c++20 -tused -A
  namespace N {
    template<typename T> struct E {};
    typedef E<int> F;
  }
  namespace M {
    typedef N::F H;
  }
  void g(N::F f) {
    typedef N::F G;
    f.M::H::~F(); // #8 ill-formed
  }
