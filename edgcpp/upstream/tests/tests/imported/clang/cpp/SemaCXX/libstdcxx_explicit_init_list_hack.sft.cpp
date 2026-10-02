//type: fp
//options:  --c++11 -D__GLIBCXX__=20100000L
# 1 "SemaCXX/libstdcxx_explicit_init_list_hack.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/libstdcxx_explicit_init_list_hack.cpp" 2
# 19 "SemaCXX/libstdcxx_explicit_init_list_hack.cpp"
# 1 "SemaCXX/libstdcxx_explicit_init_list_hack.cpp" 1
# 7 "SemaCXX/libstdcxx_explicit_init_list_hack.cpp" 3
namespace std {
namespace __debug {
template <class T>
class vector {
public:
  explicit vector() {}
};
}
}
# 20 "SemaCXX/libstdcxx_explicit_init_list_hack.cpp" 2

struct { int a, b; std::__debug::vector<int> c; } e[] = { {1, 1} };

decltype(new std::__debug::vector<int>[1]{}) x;
