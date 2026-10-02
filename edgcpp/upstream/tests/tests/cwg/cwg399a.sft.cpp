//type:fp
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
    f.G::~E(); // #1
    f.G::~G(); // #3
    f.N::F::~F(); // #5
    f.M::H::~H(); // #10
  }

//cwg: 399
//title: Destructor lookup redux
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23832
