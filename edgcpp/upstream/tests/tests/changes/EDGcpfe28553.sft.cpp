//type:fp
//options_all:--c++11
//remark:Variadic expansion of attributes
// 12/1/25  [EDGcpfe/28553]
//
// Variadic expansion of attributes
//
// The front end previously failed to apply the expanded attribute to the instance
// of g.  That is now fixed.
template<int ...Is> [[gnu::aligned(Is) ... ]] void g() {}
int main() {
  g<2, 4, 8>();
}
