//type:fp
//options_all:--gcc --gn 90300 --c18
//remark:[6.3] GNU C __auto_type and cv-qualifiers
// 11/11/21 [EDGcpfe/24809]
//
// GNU C __auto_type and cv-qualifiers
//
// The front end issued a spurious error when the GNU C __auto_type specifier is
// combined with cv-qualifiers.
//
// This is now fixed.
void g(int i) {
  __auto_type const x = i;  // Previously a spurious error.  Now okay.
}
