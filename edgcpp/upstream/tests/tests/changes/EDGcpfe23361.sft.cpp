//type:fp
//options_all:--microsoft_version=1936
//remark:[6.2] Spurious error with DMI and nested types
// 10/5/20  [EDGcpfe/23361]
//
// Spurious error with DMI and nested types
//
// The change for EDGcpfe/19729 (in version 6.1) introduced a regression where
// the front end would issue spurious errors on certain data member initializers
// that used nested types.
//
// This is now fixed.
struct A {
  class B {
    int m_0{0};
  };
  B b1{};
  B b2{}; // Spurious "constructor cannot be used in an initializer for its
          // own data member" error
};
