//type:fp
//options_all:--c++20 --c++11
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
void g() {
  for (int i = decltype(i)();;) {}  // Previously a spurious error, now okay.
}
