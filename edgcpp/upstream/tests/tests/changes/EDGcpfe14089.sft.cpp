//type:fp
//remark:[4.8] Abort on mem-initializer with destructor when exceptions are disabled
// 5/28/13  [EDGcpfe/14089]
//
// Abort on mem-initializer with destructor when exceptions are disabled
//
// The initializer changes of version 4.5 (see entries for EDGcpfe/9170)
// introduced a bug in C++ modes that disable exception handling (including
// default C++ mode), causing IL lowering to abort (in lower_dynamic_init) when
// processing a mem-initializer for a member whose type has a nontrivial
// destructor.
//
// This is now fixed.
struct D { ~D(); };
struct S {
  D d;
  S(D x): d(x) {}  // This previously triggered an abort in lowering when
};                 // exception handling is disabled.
