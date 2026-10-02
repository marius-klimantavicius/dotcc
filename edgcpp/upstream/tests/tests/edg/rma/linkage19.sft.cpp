//options_all:-r -x -tused
//options: --strict;cn

// 7.5
// linkage specs and overload on return type

typedef void (*fp)(double);

//test 1
extern "C" int f(fp) ; 
extern "C" void f(fp);

