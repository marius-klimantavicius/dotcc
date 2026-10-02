//type:fp
//remark:[4.2] Abort on aggregate initializer involving an unnamed bit field
// 6/2/10   [EDGcpfe/10721]
//
// Abort on aggregate initializer involving an unnamed bit field
//
// The front end aborted during lowering in lower_dynamic_init_aggregate_constant
// ("have constant, no field") in some cases with an aggregate initializer that
// does not initialize all the fields of a class type that also includes an
// unnamed bit field.
//
// This is now fixed.
struct D { D(); };
struct A {
  int i;
  D   d;
  int :0;
};
int main() { A x = { 0 }; }  // Previously triggered an internal error.
