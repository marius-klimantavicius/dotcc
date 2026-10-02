//type:fp
//options_all:--c++23
//remark:[6.3] C++23: Explicit-this member functions
// 11/3/21  [EDGcpfe/24781]
//
// C++23: Explicit-this member functions
//
// In C++23 mode, the front end now accepts member functions with an explicit
// "this" parameter.  This also applies to lambda expressions, which therefore can
// be made recursive.
//
// This feature was added to the working paper for the next standard by the
// standardization committee's paper P0847R7.
constexpr auto lm = [](this auto &self, int i)->int {
                      return i>1 ? i*self(i-1) : 1;
                    };
static_assert(lm(9) == 362880);
