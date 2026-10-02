//type:fn
//options_all:--c++20
//remark:[6.7] Abort on CTAD involving aggregate class with dependent base
// 4/18/24  [EDGcpfe/27135]
//
// Abort on CTAD involving aggregate class with dependent base
//
// Previously this aborted while the front end tried to generate an implicit
// deduction guide for the aggregate template S.  That is now fixed.
template<int V> struct Val { static constexpr int value = V; };
template<typename V> struct S: public Val<__is_constructible(V)> {};
static_assert(S(1), "");  // Previously aborted.  Now an error.
