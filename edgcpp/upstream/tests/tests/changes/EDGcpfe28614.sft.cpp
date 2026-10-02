//type:fp
//options_all:--g++ --c++23
//remark:Constant-evaluation of relational operators involving NaNs
// 12/19/25 [EDGcpfe/28614]
//
// Constant-evaluation of relational operators involving NaNs
//
// Some constant-evaluation of relational operators involving NaN values had
// produced incorrect results.
static_assert (!(__builtin_nanf("") <= 0.0f));
