//type:fp
//options_all:--gn 90200
//remark:[6.3] Nonstandard anonymous unions with base classes
// 3/11/21  [EDGcpfe/23822,EDGcpfe/23917,EDGcpfe/23998]
//
// Nonstandard anonymous unions with base classes
//
// Previously, nonstandard anonymous unions that are really struct types -- in
// modes that accept that extension -- were never permitted to have base classes.
// Now that restriction only applies to GNU C++ mode with gnu_version < 30400.
// In other GNU C++ modes, the anonymous struct is only required to be a trivially
// copyable type, and in Clang and Microsoft C++ modes even that requirement is
// lifted.
struct B {};
struct S {
  struct : B {  // Previously a warning, because the nonstandard
    int i;      // anonymous union-like construct was not recognized.
  };
};
int main() {
  S s;
  s.i = 1; // Previously an error.  Now okay in GNU, Clang, and Microsoft
}          // C++ modes.
