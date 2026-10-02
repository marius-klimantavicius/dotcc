//type:fp
//remark:[4.1] C++-generating back end: Abort or nonterminating loop with Microsoft-mode
// 6/29/09  [EDGcpfe/9918]
//
// C++-generating back end: Abort or nonterminating loop with Microsoft-mode
// in-class specialization
//
// In Microsoft mode the C++-generating back -- when compiled with
// PROTOTYPE_INSTANTIATIONS_IN_IL set to TRUE -- sometimes aborted with an
// internal error in gen_variable_decl (cp_gen_be.c) when handling certain
// in-class specializations of member class templates.  Other similar cases
// instead resulted in a nonterminating loop.  (In-class specializations
// are a Microsoft extension.)
//
// This is now fixed.  (The underlying problem was the incorrect generation of
// source sequence entries for prototype instantiations, rather than a bug in
// the C++-generating back end proper.)
template<typename T> struct S {
  template<int> struct N {};
  template<> struct N<0> {
    template<typename> struct I {};
  };
};
template struct S<int>;  // Triggered an "infinite loop" in cp_gen_be.c.
