//type:fp
//options:--c++14 -w

namespace minimal {
  namespace ns1 {
    template<typename> int f();
  }
  namespace ns2 {
    using namespace ns1;
  }
  auto *f(int i) {
    return ns2::f<decltype(i)>;
  }
}
