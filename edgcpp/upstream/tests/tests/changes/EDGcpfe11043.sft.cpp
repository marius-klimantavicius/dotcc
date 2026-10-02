//type:fp
//remark:[4.3] C++-generating back end aborts on old-style member function specialization
// 10/12/10 [EDGcpfe/11043]
//
// C++-generating back end aborts on old-style member function specialization
//
// In version 4.2, the C++-generating back end aborted in Microsoft mode with
// microsoft_version >= 1310 when attempting to render the definition of an
// old-style specialization of a member function of a class template.  (The
// abort occurred in bypass_prototyped_param_src_seq_entries in cp_gen_be.c.)
//
// This is now fixed.  (This is a regression introduced in version 4.2 by the
// changes for EDGcpfe/10128; see the Changes entry of 12/16/09.)
template<typename> struct S {
  void f();
};
void S<int>::f() {  // The C++-generating back end previously aborted
  int x = 3;        // when attempting to render this definition.
}
