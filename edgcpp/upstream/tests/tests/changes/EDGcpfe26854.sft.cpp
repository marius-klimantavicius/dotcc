//type:fp
//options_all:--targ win64 --set_flag force_ms_type_info_not_in_namespace_std --microsoft_version=1600 --microsoft_build_number=30319 --pending_instantiations 2000 --rtti --wchar_t_keyword --new_for_init -w
//remark:[6.7] Microsoft compatibility: Ambiguous member access and using-declarations
// 12/18/23 [EDGcpfe/26854]
//
// Microsoft compatibility: Ambiguous member access and using-declarations
//
// The changes for EDGcpfe/16204,EDGcpfe/26719 contained an error causing some
// cases accepted by MSVC to still fail.
//
// That bug is now fixed.
struct B { void f() const; };
struct B1: B {}; 
struct B2: B {}; 
struct D: B1, B2 {
  using B1::f;  // MSVC uses this to disambiguate access to D::f.
};
struct X: D {
  using D::f;
};
struct E: D {};
void f(E *p) {
  p->f();  // Still considered ambiguous because of bug induced by the
}          // using-declaration in X (which is not actually related to E).
