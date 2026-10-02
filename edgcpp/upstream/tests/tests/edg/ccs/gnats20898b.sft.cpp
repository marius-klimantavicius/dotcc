//options:--gnu_version 80100
//options_all:--c++11 --instantiate=used

struct A { };
template<int idx>
void foo (A *x, A *y) {
  *x = y[idx*1];
}
auto ptr = &foo<0>;
