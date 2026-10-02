//type:fp
//options_all:--strict
//remark:[4.1] Local type with a base class can result in an assertion failure
// 5/18/09  [EDGcpfe/9822]
//
// Local type with a base class can result in an assertion failure
//
// In configurations that use lowering, the combination of a local type with
// a virtual base class resulted in an assertion failure (in set_parent_scope)
// when lowering of the local function is delayed (e.g., in strict mode).
//
// Also, this example failed in the same way in some IA-64 ABI configurations:
struct A {};
void f() {
  struct B {
    B() {
      struct C : virtual A {};
    }
  };
}
