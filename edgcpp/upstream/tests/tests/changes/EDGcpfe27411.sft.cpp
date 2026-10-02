//type:fp
//options_all:--c++20
//remark:[6.8] Spurious ambiguity between built-in and user-declared comparison operators
// 4/28/25  [EDGcpfe/27411,EDGcpfe/27612,EDGcpfe/28129]
//
// Spurious ambiguity between built-in and user-declared comparison operators
//
// Previously, this resulted in a spurious ambiguity error due to a bug in the
// implementation of overload resolution for C++20 comparison operators (which
// involves, among other options, the potential rewriting of "!=" in terms of
// "==").  That is now fixed.
class S {
  operator char*();
  operator void*();
  friend bool operator==(S, char);
} s;
bool r =  s != 0;  // Previously an error.  Now okay.
