//type:fp
//options_all:-I. --microsoft_version=1925 --ms_c++17 --no_ms_permissive
//remark:[6.0] Direct-initialization in explicit variable template specializations
// 11/14/19 [EDGcpfe/21986]
//
// Direct-initialization in explicit variable template specializations
//
// The front end previously treated direct-initialization in explicit variable
// template specializations as if it were copy-initialization.  That,
// caused erroneous type deduction in some cases.
//
// That is now fixed.
#include <initializer_list>
template<typename> constexpr auto &v{"x"};
template<> inline constexpr auto &v<bool>{"y"};  // Previously an error.
                                                 // Now okay.
