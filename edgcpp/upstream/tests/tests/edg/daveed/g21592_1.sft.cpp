//remark:constinit
//options:--c++17;fn:--c++20;fp:--c++20 -DNEG;fn

constinit int i = 42;
constinit int j;
#if NEG
int f();
constinit int k = f();
#endif
