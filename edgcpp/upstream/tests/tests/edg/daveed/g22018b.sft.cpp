//remark:Multistage substitution
//options:--c++17;fp

template <bool> struct a;
template <typename> struct b {};
struct C {
  template <typename> static bool c();
};
class d {
public:
  template <typename g, typename a<C::c<g>()>::h = true> d(b<g>);
  template <typename g> void operator=(b<g>);
};
d i();
b<int> j;
void k() { i() = j; }
