//type: fp
//options:  --c++17
# 1 "SemaCXX/builtin-exception-spec.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 479 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/builtin-exception-spec.cpp" 2



# 1 "SemaCXX/Inputs/malloc.h" 1
extern "C" {
extern void *malloc (long unsigned int __size) throw () __attribute__ ((__malloc__)) ;
}
# 5 "SemaCXX/builtin-exception-spec.cpp" 2

extern "C" {
void *malloc(long unsigned int);
}
