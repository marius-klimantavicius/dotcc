//type:fp
//options_all:--microsoft
//remark:Microsoft compatibility: __declspec on statement
// 3/23/26  [EDGcpfe/28665]
//
// Microsoft compatibility: __declspec on statement
//
// The processing of a __declspec construct on a statement had caused some
// irregularities during the parsing, resulting in some inconsistencies in the
// IL.  In the following example (with --microsoft), only one stmk_asm
// statement had been generated:
void f() {
  __declspec() asm("nop");
}
