//type:fp
//remark:[4.3] Invalid IL when HANDLE_VIRTUAL_BASES_IN_COMPLETE_CTOR_DTORS is TRUE
// 3/11/11  [EDGcpfe/11490]
//
// Invalid IL when HANDLE_VIRTUAL_BASES_IN_COMPLETE_CTOR_DTORS is TRUE
//
// In IA-64 ABI configurations that use lowering where
// HANDLE_VIRTUAL_BASES_IN_COMPLETE_CTOR_DTORS is TRUE, a constructor init where
// an argument is passed via copy constructor had resulted in invalid IL
// (where the expression type doesn't match the parameter type).  In C generating
// back end configurations, this had resulted in a "check_type_of_variable_node"
// internal error.
struct A {
  A();
  A(const A&);
};
struct B {
  B(A) {}
};
struct D : virtual B {
  D(A a) : B(a) {}
};
A a;
D d(a);
