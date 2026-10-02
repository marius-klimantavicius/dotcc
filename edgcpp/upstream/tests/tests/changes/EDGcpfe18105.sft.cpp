//type:fp
//options_all:--c++11
//remark:[4.14] Assertion failure in attach_tag_attributes
// 3/15/17  [EDGcpfe/18105]
//
// Assertion failure in attach_tag_attributes
//
// The changes for EDGcpfe/17548 (in 4.13) caused an assertion failure
// (in attach_tag_attributes) when parsing a qualified redeclaration that
// included attributes (or alignas).  The failure is only seen in configurations
// where GENERATE_SOURCE_SEQUENCE_LISTS is TRUE.
struct alignas(int) S2 {};
struct alignas(int) ::S2;
