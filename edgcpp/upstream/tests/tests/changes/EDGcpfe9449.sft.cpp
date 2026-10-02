//type:fp
//options_all:--microsoft
//remark:[4.1] Spurious errors on some uses of __sptr/__uptr in Microsoft C++ mode
// 1/19/09  [EDGcpfe/9449]
//
// Spurious errors on some uses of __sptr/__uptr in Microsoft C++ mode
//
// Although the front end supports __sptr/__uptr in Microsoft mode (see the
// entry of 5/22/07), some valid uses triggered spurious errors in Microsoft
// C++ mode.
//
// This is now fixed.
int* __uptr p = (int* __uptr)(0xFFFFFFFF);  // Previously a spurious error.
