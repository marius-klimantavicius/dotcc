//type:fp
//options_all:--c++14
//remark:[6.8] Memory region violation for unevaluated operands
// 4/7/25   [EDGcpfe/20625,EDGcpfe/21181,EDGcpfe/24947,EDGcpfe/26668,
//           EDGcpfe/26839]
//
// Memory region violation for unevaluated operands
//
// Previously, this example could trigger an abort in the front end due to a
// violation of the rule that file-scope IL entries (in this case, the template
// argument for C) cannot contain pointers to function-scope IL entries (in this
// case, the a_variable entry representing i).  That is now fixed.
template<int> struct C { };
void g(int i) {
  [] (auto j) -> C<sizeof(i + j)> { return {}; };
}
