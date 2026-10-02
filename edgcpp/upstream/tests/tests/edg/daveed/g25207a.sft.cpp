//remark:Incomplete type operands in templates
//options:--g++ --no_defer;fp:--c++11 -A;fn

struct I;
template<typename T> void g(I *p) {
  auto x = p[2];
}
