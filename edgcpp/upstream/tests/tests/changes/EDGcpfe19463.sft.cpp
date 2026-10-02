//type:fp
//options_all:--g++
//remark:[5.0] C++-generating back end, GNU compatibility: compound literals and constructors
// 3/28/18  [EDGcpfe/19463]
//
// C++-generating back end, GNU compatibility: compound literals and constructors
//
// The C++-generating back end previously failed an assertion (in
// gen_compound_literal) when C++ source has a C99-style compound literal that
// results in invoking a constructor (a situation that can arise in g++ mode).
// This is now fixed.
struct S1 { S1(unsigned); };
S1 d = (S1){1};  // Previously resulted in an assertion failure
