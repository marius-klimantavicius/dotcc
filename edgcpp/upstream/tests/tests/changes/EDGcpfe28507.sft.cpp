//type:fn
//options_all:--c++17 --microsoft
//remark:[6.8] Microsoft and GNU compatibility: Literal types
// 10/28/25 [EDGcpfe/28507]
//
// Microsoft and GNU compatibility: Literal types
//
// C is normally not a literal type because its default constructor doesn't
// initialize j (and thus is not constexpr) and it is not an aggregate class
// (because it has private members).  GCC and MSVC, however, do treat C as a
// literal type and the front end now emulates that behavior.
class C {
  int i = 0, j;
};
static_assert(!__is_literal_type(C), "");  // Now an error in GNU and
                                           // Microsoft modes.
