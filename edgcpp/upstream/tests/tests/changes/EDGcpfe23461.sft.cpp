//type:fp
//options_all:--c++11 --clang_version=90000
//remark:[6.2] Abort during processing of Clang enable_if attribute
// 10/19/20 [EDGcpfe/23461]
//
// Abort during processing of Clang enable_if attribute
//
// The use of Clang's enable_if attribute could result in an internal error in
// release_local_constant.
//
// This is now fixed.
constexpr int g() __attribute((enable_if(0, ""))) { return 0; }
constexpr int g() { return 1; }
static_assert(g() == 1, "");  // Previously an internal error.  Now okay.
