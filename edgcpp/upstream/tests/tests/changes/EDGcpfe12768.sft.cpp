//type:fp
//options_all:--g++
//remark:[4.5] GNU C++ compatibility: Extraneous comma after member declarator
// 3/20/12  [EDGcpfe/12768]
//
// GNU C++ compatibility: Extraneous comma after member declarator
//
// In GNU C++ mode with gnu_version >= 30400, the front end now accepts an
// extraneous comma after a class member declarator (with a warning), if that
// comma is followed by a semicolon.
struct S {
  S(),;    // Now accepted with a warning in GNU C++ mode.
  int i,;  // Ditto.
};
