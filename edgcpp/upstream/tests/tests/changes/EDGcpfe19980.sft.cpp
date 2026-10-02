//type:fp
//options_all:--gnu=80200
//remark:[5.1] GNU compatibility: specifying alignment of enum type
// 8/17/18  [EDGcpfe/19980]
//
// GNU compatibility: specifying alignment of enum type
//
// The C++ standard says that when multiple alignments are specified the resulting
// alignment is the strictest, but, presumably due to a bug, GNU appears to use
// the last alignment.  In cases where this is applied to an enum type, GNU also
// appears to ignore the alignment in cases where it is smaller than the alignment
// of the base type of the enum type.  The front end now emulates that behavior
// (with a warning).
enum alignas(16) alignas(1) E1 { };   // should have alignment of 16
enum alignas(16) alignas(8) E2 { };   // should have alignment of 16
static_assert(alignof(E1) == alignof(int), "");
static_assert(alignof(E2) == 8, "");
