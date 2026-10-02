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
    f.N::F::~E(); // #4 ill-formed
  }
