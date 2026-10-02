//type:fp
//options_all:--c++11
//remark:Name lookup in instantiated alias template declaration
// 4/15/26  [EDGcpfe/28793]
//
// Name lookup in instantiated alias template declaration
//
// Previously, the front end failed to exclude using directives declared after an
// alias template when looking up names during an alias template instantiation.
namespace ns1 { template<typename T> using A = T; }
namespace ns2 { template<typename T> using A = void; }
namespace ns {
  using namespace ns1;
  template<typename> struct C {
    template<typename T> using type = A<T>;  // Previously ambiguous, now
                                             // okay.
  };
}
using namespace ns2;
ns::C<int>::type<int> i = 1;
