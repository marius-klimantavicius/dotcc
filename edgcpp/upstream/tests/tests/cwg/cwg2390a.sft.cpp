//type:fp
//options_all:--c++20 -A
#define f03(name) name
#if __has_cpp_attribute ( f03(deprecated) )
#else
#error Unexpected
#endif
