//type:fp
//options_all:--microsoft_version 1900
//remark:[5.1] Abort on inheriting constructor in dllexport class
// 9/24/18  [EDGcpfe/17962,EDGcpfe/20184]
//
// Abort on inheriting constructor in dllexport class
//
// The front end previously failed to propagate the "dllexport" property of a
// class type to an inheriting constructor declaration that it contains.  In turn,
// that caused an internal error in force_definition_of_generated_exported_members
// (class_decl.c).
//
// That is now fixed.
struct B {
  template<typename T> constexpr B(T) {}
};
struct __declspec(dllexport) D: B {
  using B::B;  // Previously triggered an abort in class_decl.c.
};
