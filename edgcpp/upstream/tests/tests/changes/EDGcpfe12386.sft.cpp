//type:fp
//remark:[4.12] Casting a template-id for a function template instance to void
// 5/25/16  [EDGcpfe/12386,EDGcpfe/16977,EDGcpfe/17015]
//
// Casting a template-id for a function template instance to void
//
// The front end previously issued a spurious error when a template-id denoting a
// single template instance is cast to void.
//
// This is now fixed.
template<int i> void f() {}
int main() {
  static_cast<void>(&f<1>);  // Previously a spurious error; now okay.
}
