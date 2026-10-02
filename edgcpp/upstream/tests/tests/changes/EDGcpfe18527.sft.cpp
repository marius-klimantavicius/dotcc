//type:fp
//options_all:--c++17
//remark:[5.1] Capturing of *this
// 12/4/18  [EDGcpfe/18527,EDGcpfe/20367,EDGcpfe/20522,EDGcpfe/20523]
//
// Capturing of *this
//
// When capturing *this, the front end often lost track of the correct types
// involved, resulting in spurious errors.
//
// This is now fixed.
struct S {
  void f() {
      [*this]{ [this] {}; };  // Previously a spurious error about not being
                              // able to convert from S const* to S *const.
  }
};
