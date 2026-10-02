//type: fn
//options:  --c++11
# 1 "SemaCXX/libstdcxx_atomic_ns_hack.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/libstdcxx_atomic_ns_hack.cpp" 2
# 30 "SemaCXX/libstdcxx_atomic_ns_hack.cpp"
# 1 "SemaCXX/libstdcxx_atomic_ns_hack.cpp" 1
# 15 "SemaCXX/libstdcxx_atomic_ns_hack.cpp" 3

namespace std {
namespace __atomic0 {
typedef int foobar;
}
namespace __atomic1 {
typedef void foobar;
}

inline namespace __atomic0 {}
}
# 31 "SemaCXX/libstdcxx_atomic_ns_hack.cpp" 2

std::foobar fb;
