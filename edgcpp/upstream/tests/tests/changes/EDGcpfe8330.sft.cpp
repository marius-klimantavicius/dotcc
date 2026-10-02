//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft compatibility: Calling conventions and pointer-to-member-function
// 1/2/09   [EDGcpfe/8330]
//
// Microsoft compatibility: Calling conventions and pointer-to-member-function
// compatibility
//
// In Microsoft mode, the front end previously ignored calling conventions when
// checking compatibility of pointer-to-member-function initializations and
// assignments.
//
// This is now fixed: Incompatible calling conventions are errors in such cases.
struct S { void f(); };              // Default convention (__thiscall).
void (__stdcall S::*pmf)() = &S::f;  // Error when the 4.1 change was made;
                                     // accepted again in later versions.
