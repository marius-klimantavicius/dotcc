//type:fp
//options_all:--clang_v 220100
//remark:GCC/Clang compatibility: CTAD for alias templates in pre-C++20 modes
// 5/21/26  [EDGcpfe/28849]
//
// GCC/Clang compatibility: CTAD for alias templates in pre-C++20 modes
//
// In Clang 19+ modes, the front end now accepts the C++20 class template argument
// deduction (CTAD) for alias templates feature with a warning in pre-C++20 modes.
// In GCC 10+ modes, the feature is accepted only for simple alias templates (see
// the entry for EDGcpfe/28073).
template<class T> struct Box { Box(T); };
template<class T> using Alias = Box<T>;
void test() { Alias a(42); }
