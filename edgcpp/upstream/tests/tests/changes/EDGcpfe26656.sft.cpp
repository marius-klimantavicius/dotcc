//type:fp
//options_all:--microsoft
//remark:Spurious error on casting a function to a reference-to-pointer
// 4/6/26   [EDGcpfe/26656,EDGcpfe/27450]
//
// Spurious error on casting a function to a reference-to-pointer
//
// Previously, this was spuriously diagnosed as a conversion error.  That is now
// fixed.
void g() noexcept;
using PF = void (*)();
PF const &rrpf = static_cast<PF const&&>(g);
