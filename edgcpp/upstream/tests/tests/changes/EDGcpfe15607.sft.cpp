//type:fp
//options_all:--c++11
//remark:[4.10] Binding a non-const constexpr reference to a numeric literal
// 12/8/14  [EDGcpfe/15607]
//
// Binding a non-const constexpr reference to a numeric literal
//
// The front end previously issued a spurious diagnostic on an attempt to bind
// a constexpr reference to a non-const type to a numeric literal.  This is
// now fixed.
constexpr int &&r = 3;  // Previously an error, now accepted
