//type:fp
//options_all:--c++20
//remark:Spurious error during disambiguation of for statement
// 1/22/26  [EDGcpfe/25649,EDGcpfe/28610]
//
// Spurious error during disambiguation of for statement
//
// The changes for EDGcpfe/27435 (in version 6.7) introduced a regression during
// disambiguation of a for statement containing a declaration with a constrained
// placeholder type.
//
// Additionally, this also fixes an issue where a decltype specifier refers to a
// declarator in the same declaration of a for statement.
// --c++11:
template<typename, int> concept C = true;
void f(auto t) {
  for (C<1> auto v : t) { }  // Previously a spurious error, now okay.
}
