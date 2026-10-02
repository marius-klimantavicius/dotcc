//type:fp
//options_all:--gnu_version=120200 --c++20 -tused -w
//remark:Abort in i_copy_dynamic_init on lambda in template argument list
// 4/17/26  [EDGcpfe/28494,EDGcpfe/28563]
//
// Abort in i_copy_dynamic_init on lambda in template argument list
//
// This previously aborted in i_copy_dynamic_init (in file il.c) while copying
// the default argument of the X::X(D) constructor.  That is now fixed.
struct D { ~D(); };
struct X {
  X(D l = D()) {}
};
template<typename> int v;
int main() {
  v<decltype( []{ X(); } )>;
}
