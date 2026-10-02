//type:fp
//options_all:--c++11 --clang
//remark:[4.13] Clang compatibility: enable_if attribute
// 2/13/17  [EDGcpfe/17738]
//
// Clang compatibility: enable_if attribute
//
// Preliminary support has been added for the enable_if attribute in Clang C++
// mode.
//
// Note that the attribute permits overloading functions that would otherwise not
// be distinguishable and that, all other things being equal, the presence of an
// enable_if attribute in a viable candidate makes the candidate preferable over
// one without the attribute.
//
// A few caveats:
// Despite these limitations, this support is sufficient to handle the uses of
// this attribute in current standard header files shipped with Clang.
void f() __attribute((enable_if(true, ""))) {}
void f() = delete;
int main() {
  f();  // Now accepted in Clang C++11 mode.
}
