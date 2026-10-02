//type:fp
//options_all:--microsoft
//remark:[6.4] Microsoft compatibility: __declspec(nothrow) and noexcept
// 8/24/22  [EDGcpfe/15165,EDGcpfe/19247,EDGcpfe/21823,EDGcpfe/25560]
//
// Microsoft compatibility: __declspec(nothrow) and noexcept
//
// In Microsoft C++ modes, __declspec(nothrow) applied to a function now implies
// "noexcept".
__declspec(nothrow) void g() {}
void (*f)() noexcept = g;  // Previously an error.  Now okay.
