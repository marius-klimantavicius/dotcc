//options_all:-r -x -tused
//options: --strict;cn

// Overloading and linkage specifiers
extern "C" int f();
extern "C" int f(int);
extern "C++" int g();
extern "C++" int g(int);
extern int h();
extern "C++" int h(int);
extern "C" int h(int,int);

