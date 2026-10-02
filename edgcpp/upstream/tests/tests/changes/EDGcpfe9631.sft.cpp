//type:fp
//options_all:--g++
//remark:[4.1] GNU C++ compatibility: Member declarations that don't declare anything
// 3/23/09  [EDGcpfe/9631]
//
// GNU C++ compatibility: Member declarations that don't declare anything
//
// In GNU C++ mode, the front end now accepts (with a warning) a member
// declaration that doesn't declare anything if that declaration refers to a
// class or enumeration type using an elaborated type name.
struct X;
struct S {
  struct ::X;  // Now accepted in g++ mode.
};
