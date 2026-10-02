//type:fn
//options_all:--clang
//remark:[4.10] Clang compatibility: __char16_t/__char32_t keywords
// 7/11/14  [EDGcpfe/15257]
//
// Clang compatibility: __char16_t/__char32_t keywords
//
// The clang compiler defines __char16_t and __char32_t keywords in all C++
// modes, and now so does the front end (in clang emulation mode).  The
// __char16_t and __char32_t keywords have the same meaning as the
// char16_t and char32_t keywords respectively.
//
// Also introduced during this change is the ability to specifically target
// a clang back-end (C or C++) compiler by using the new
// CLANG_IS_GENERATED_CODE_TARGET and CLANG_TARGET_VERSION_NUMBER configuration
// macros.  For the most part, these are similar to the existing
// GCC_IS_GENERATED_CODE_TARGET and GCC_IS_GENERATED_CODE_TARGET macros.
typedef __char16_t char16_t;  // Accepted when the 4.10 change was made;
typedef __char32_t char32_t;  // later versions diagnose invalid type specifiers.
