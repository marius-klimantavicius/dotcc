//type:fp
//options_all:--c++20 --g++
//remark:[6.6] GNU and Microsoft compatibility: Casting of pointer-to-member values
// 8/31/23  [EDGcpfe/26640]
//
// GNU and Microsoft compatibility: Casting of pointer-to-member values
//
// Ordinarily this is an error because the C-style cast is a reinterpret-like
// cast and such casts are not permitted in constant expressions.  However, GCC
// and MSVC do accept this example.  The front end now approximates that behavior.
// (Note that before the changes for EDGcpfe/26198 the front end erroneously also
// accepted this example.  In some sense this could therefore be considered a fix
// for a regression introduced in version 6.5 of the front end.)
struct B {};
using PM = void (B::*)();
struct C: B {};
struct D: C { int f(); };
static constinit const PM pm = (PM)static_cast<int (C::*)()>(&D::f);
    // Previously an error.  Now accepted in some modes.
