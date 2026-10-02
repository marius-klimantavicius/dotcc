//type:fp
//options_all:--c++20 -A
#define foo deprecated
#if __has_cpp_attribute(foo)
#else
#error Unexpected
#endif

//cwg: 2390
//title: Is the argument of __has_cpp_attribute macro-expanded?
//meeting: Cologne 07/19
//edg_status: EDGcpfe/22563
//fixed_in: 6.9
