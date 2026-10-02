//type:fp
//options_all:--ms_permissive
//remark:[6.7] Microsoft compatibility: __is_convertible_to
// 1/29/24  [EDGcpfe/21278,EDGcpfe/25457]
//
// Microsoft compatibility: __is_convertible_to
//
// In Microsoft C++ modes, the __is_convertible_to type traits helper previously
// took a Microsoft-mode anachronism into account that allows an lvalue reference
// to a non-const class type to bind to a class rvalue.  However, starting with
// version 19.00, the Microsoft compiler does not do so.
struct C {};
C &c = C();  // Anachronism still accepted in permissive Microsoft mode.
static_assert(!__is_convertible_to(C, C &), "Unexpected");
                // Previously failed in permissive Microsoft mode due to the
                // anachronism illustrated above.  Now okay.
