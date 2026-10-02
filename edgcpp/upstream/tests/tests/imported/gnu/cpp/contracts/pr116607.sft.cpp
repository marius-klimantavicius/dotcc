//type: fp
//options:  --c++20
// { dg-options "-std=c++20 -fcontracts " }
struct a {
  __attribute__((no_sanitize("")))
  int f(int) [[pre:true]];
};
int a::f(int) { return 0; }
