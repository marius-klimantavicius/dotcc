//type:fp
//remark:[4.3] C++-generating back end aborts on duplicate using-declaration
// 10/4/10  [EDGcpfe/11040]
//
// C++-generating back end aborts on duplicate using-declaration
//
// The front end previously generated an invalid list of source sequence entries
// when parsing two using-declarations in a function definition (when
// GENERATE_SOURCE_SEQUENCE_LISTS is TRUE).  This in turn triggered an internal
// error in gen_declaration ("bad entity kind on source seq list", cp_gen_be.c).
//
// This is now fixed.
void f();
void g() {
  using ::f;
  using ::f;  // Duplicate using-declaration previously triggered an
  f();        // internal error in cp_gen_be.c.
}
