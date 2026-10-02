//type:fp
//remark:[4.12] Named return value optimization and braced initializers
// 5/20/16  [EDGcpfe/17182]
//
// Named return value optimization and braced initializers
//
// The changes for EDGcpfe/16778 in version 4.11 introduced a regression by
// failing to disqualify the "named return value optimization" (see the entry of
struct S { S(); ~S(); };
S f(bool b) {
  if (!b) return {};
  S s;
  return s;
}
S r = f(false);
