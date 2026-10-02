//type: fn
//options:  --c++14
# 1 "SemaCXX/libstdcxx_gets_hack.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 432 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/libstdcxx_gets_hack.cpp" 2
# 22 "SemaCXX/libstdcxx_gets_hack.cpp"
# 1 "SemaCXX/libstdcxx_pointer_return_false_hack.cpp" 1
# 11 "SemaCXX/libstdcxx_pointer_return_false_hack.cpp" 3
namespace std {
  namespace tr1 {
    template<typename T> struct hashnode;
    template<typename T> struct hashtable {
      typedef hashnode<T> node;
      node *find_node() {


        return false;
      }
    };
  }
}
# 23 "SemaCXX/libstdcxx_gets_hack.cpp" 2

namespace foo {
  using ::gets;
}
