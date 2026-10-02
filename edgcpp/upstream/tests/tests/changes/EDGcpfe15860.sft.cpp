//type:fp
//options_all:--c++14
//remark:[4.11] Abort in C++14 mode on missing lifetime for default member initializer
// 1/19/16  [EDGcpfe/15860,EDGcpfe/16756]
//
// Abort in C++14 mode on missing lifetime for default member initializer
//
// In C++14 mode, the front end previously sometimes aborted with an internal
// error in lower_dynamic_init when an aggregate initializer relies on a
// default member initializer for a field requiring nontrivial destruction.
//
// This is now fixed.
struct D { ~D(); };
struct X {
  D d = D();
};
void g() {
  X{};  // Previously triggered an internal error in lower_dynamic_init.
}       // Now okay.
