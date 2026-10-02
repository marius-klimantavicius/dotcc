//type:fp
//options_all:--c++14
//remark:[4.10.1] Mangled encoding for "operator auto()"
// 1/28/15  [EDGcpfe/15930]
//
// Mangled encoding for "operator auto()"
//
// Previously, the mangled encoding used for an "operator auto" conversion
// function was that of the deduced type, now the mangled encoding for deduced
// "auto" types is used (i.e., "Da" in the IA-64 ABI and "u" in the Cfront ABI).
// The change is conditional on ABI_COMPATIBILITY_VERSION >= 411.  Prior to this
// change, an "operator auto()" conversion function that had returned a local
// type would generate an infinite recursion.
struct A {
  operator auto() {
    struct B {};
    return B();
  }
};
