//type:fp
//options_all:--microsoft
//remark:[4.7] Microsoft compatibility: __is_convertible_to
// 4/9/13   [EDGcpfe/13710]
//
// Microsoft compatibility: __is_convertible_to
//
// Previously, in Microsoft modes, uses of __is_convertible_to with two identical
// "void" types or two identical function types produced a "true" value.  Now it
// produces a "false" value to match the behavior of Microsoft's compilers.
typedef int F();
static_assert(!__is_convertible_to(F, F), "Should not trigger");
