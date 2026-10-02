//type:fp
//options_all:--ms_c++14 --microsoft_version=1903
//remark:[6.4] Microsoft compatibility: Static data member specialization declaration
// 10/3/22  [EDGcpfe/23505]
//
// Microsoft compatibility: Static data member specialization declaration
//
// The Microsoft compiler always treats an explicit specialization of a static
// data member of a class template as a definition, but, prior to version 1910, it
// does not do so for an explicit specialization of a static data member template.
struct C {
  template<int I>
  static const int var;
};
template<>
const int C::var<0>;  // Previously a spurious error "requires an
                      // initializer".  Now okay.
