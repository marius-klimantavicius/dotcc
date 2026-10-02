//type:fp
//options_all:--c++20 -W
//remark:[6.3] Spurious warning on by-reference capture of "this"
// 3/22/21  [EDGcpfe/24079]
//
// Spurious warning on by-reference capture of "this"
//
// Capturing "this" by copy is deprecated (see the entry for EDGcpfe/20001, etc.).
// However, the front end also issued warnings for implicit by-reference captures
// of "this".
//
// This is now fixed.
struct S {
  int g() { return  2; }
  void f() {
    int  value = 3;
    [&]{ return value+g(); };  // Previously a spurious warning.  Now okay.
  }
};
