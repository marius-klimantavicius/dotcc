//type:fp
//options_all:--microsoft_v 1921 --ms_c++17
//remark:[6.2] Microsoft C++ compatibility: __is_constructible
// 10/20/20 [EDGcpfe/22089]
//
// Microsoft C++ compatibility: __is_constructible
//
// In Microsoft C++ modes, the __is_constructible type traits helper previously
// took into account a Microsoft-mode anachronism that allows an lvalue reference
// to a non-const class type to bind to a class prvalue.  However, the Microsoft
// compiler does not do so.
struct S { S(S&); };
S make_S();
S s(make_S());  // Anachronism still accepted in Microsoft mode.
static_assert(!__is_constructible(S, S));
                // Previously failed in Microsoft mode due to the anachronism
                // illustrated above.  Now okay.
