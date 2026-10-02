//type:fp
//options_all:--c++11
//remark:[4.10.1] Effect of multiple alignas specifiers on a class or enum declaration
// 1/7/15   [EDGcpfe/15589]
//
// Effect of multiple alignas specifiers on a class or enum declaration
//
// When multiple C++11 alignas specifiers are used for a class or enum
// declaration, the front end now retains the strictest (i.e., largest)
// alignment value as required by the C++ standard (except in GNU C++11 mode,
// where the previous behavior of keeping the value of the last specifier
// remains for compatibility purposes).
//
// (For variables and nonstatic data members, the strictest alignment specifier
// was already correctly recorded.)
struct alignas(4) alignas(16) alignas(8) X {};
static_assert(alignof(X) == 16, "Unexpected");  // Now accepted, except in
                                                // GNU C++11 mode.
