//type:fp
//remark:[5.1] Invalid source sequence entry for friend declaration
// 9/13/18  [EDGcpfe/19553]
//
// Invalid source sequence entry for friend declaration
//
// In configurations with GENERATE_SOURCE_SEQUENCE_LISTS set to TRUE, the front
// end failed to correctly record a source sequence entry for a friend declaration
// referring to a generated member.  Instead, a source sequence entry was created
// that did not point to an IL entry and that in turn likely resulted in an abort
// later on (access through a null pointer).
//
// That is now fixed.
struct B { B& operator=(B const&); };
struct D: B {};
struct S {
  friend D& D::operator=(D const&);  // Previously resulted in invalid IL.
};
