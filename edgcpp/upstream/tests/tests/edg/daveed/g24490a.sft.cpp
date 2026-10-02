//remark:Large literals
//options:--clang -DCLANG;fn:--clang -DCLANG --diag_warn=23;fp:--g++;fp:--g++ --diag_error=23;fn
//options_all:--c++11


auto arr = 0x10000000000000000;

#ifdef CLANG
extern unsigned long long arr;
#else
extern int arr;
#endif
