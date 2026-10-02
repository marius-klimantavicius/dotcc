//type:fp
//options_all:--microsoft_version 1900
//remark:[4.11] Microsoft emulation: Spurious error on local class with alignas
// 10/15/15 [EDGcpfe/16572]
//
// Microsoft emulation: Spurious error on local class with alignas
//
// When the keyword alignas appears on a local class definition in Microsoft
// emulation mode, a spurious error had been reported.  Now fixed.
// with --microsoft_version 1900:
void f() {
  struct alignas(16) A {};
}
