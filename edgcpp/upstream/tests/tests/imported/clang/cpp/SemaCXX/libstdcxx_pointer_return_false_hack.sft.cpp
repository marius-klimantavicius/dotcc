//type: fn
//options:  --c++11
# 1 "SemaCXX/libstdcxx_pointer_return_false_hack.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 424 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/libstdcxx_pointer_return_false_hack.cpp" 2
# 28 "SemaCXX/libstdcxx_pointer_return_false_hack.cpp"
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
# 29 "SemaCXX/libstdcxx_pointer_return_false_hack.cpp" 2

auto *test1 = std::tr1::hashtable<int>().find_node();

void *test2() { return false; }
