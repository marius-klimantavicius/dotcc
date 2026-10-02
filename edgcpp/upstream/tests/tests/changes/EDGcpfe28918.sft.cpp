//type:fp
//options_all:--gn 999999
//remark:GCC compatibility: PODness and [[no_unique_address]
// 7/6/26   [EDGcpfe/28918]
//
// GCC compatibility: PODness and [[no_unique_address]
//
// This fails with the EDG front end as well as with Clang, because PodOrNot is
// considered a "POD for layout purposes": Ignoring the attributes, PodOrNot is
// clearly a struct that could be expressed in C.  The tail padding of such a
// "POD" class cannot be reused by a derived class or a [[no_unique_address]]
// member.  However, it appears GCC treats the [[no_unique_address]] attribute in
// PodOrNot as an aspect that disqualifies PodOrNot from being a "POD for layout
// purposes".  That in turn allows the three bytes of tail padding of PodOrNot to
// be used to store the members d, e, and f of S.  The front end now emulates
// that behavior in GCC compatibility mode.
struct PodOrNot {
  int i;
  [[no_unique_address]] char c;
};
struct S {
  [[no_unique_address]] PodOrNot x;
  char d, e, f;
};
static_assert(sizeof(S) == 8);
