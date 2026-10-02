//type:fp
//options_all:--c++14 -w
//remark:[6.0] Spurious failure to constant-evaluate constexpr constructor call
// 10/3/19  [EDGcpfe/19146,EDGcpfe/19818,EDGcpfe/20689,EDGcpfe/21875]
//
// Spurious failure to constant-evaluate constexpr constructor call
//
// The front end previously sometimes failed the constant-evaluation of constexpr
// constructor calls, leading to spurious errors about accessing "expired
// storage".
//
// That is now fixed.
constexpr int id(int p) { return p; }
struct S {
  int v1, v2;
  constexpr S(int p) : v1{p}, v2{this->v1} {}
};
constexpr int g(int p) {
  S s(id(p));
  return s.v2;
}
constexpr int r = g(0);  // Previously triggered an error.  Now okay.
