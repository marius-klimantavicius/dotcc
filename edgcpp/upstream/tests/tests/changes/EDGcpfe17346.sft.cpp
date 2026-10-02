//type:fp
//options_all:--gnu=60100
//remark:[4.12] Assertion failure on cv-qualified _Complex type in aggregate
// 6/21/16  [EDGcpfe/17346]
//
// Assertion failure on cv-qualified _Complex type in aggregate
//
// An initial value for a member of an aggregate where the member has a
// cv-qualified _Complex type had resulted in an assertion failure (in
// aggr_init_complex).  Now fixed.
struct S {
  const _Complex float cf;
};
void f() {
  const S s = {{0.0f, 0.0f}};
}
