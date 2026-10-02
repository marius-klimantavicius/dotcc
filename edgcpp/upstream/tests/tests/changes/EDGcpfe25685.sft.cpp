//type:fp
//options_all:--gn 110999
//remark:[6.8] Deducing array size in new-expressions
// 7/28/25  [EDGcpfe/25685,EDGcpfe/27368]
//
// Deducing array size in new-expressions
//
// The changes for EDGcpfe/20914 implemented support for deducing an array size
// from a new expression.
//
// Those changes limited this feature to C++20 mode because the committee paper
// that enabled it (P1009R2) was voted on during the C++20 standardization cycle.
// However, the paper was voted as the resolution to a defect report and thus is
// intended to apply to all language modes that allow initializers for arrays in
// such contexts.  The front end therefore now accepts cases such as the example
// above in all C++11 (and later) modes.
char *str = new char[]{ "message" };
