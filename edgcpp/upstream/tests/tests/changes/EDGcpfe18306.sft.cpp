//type:fp
//remark:[4.14] Interpreted base-class cast sometimes fails to preserve null pointer
// 4/28/17  [EDGcpfe/18306]
//
// Interpreted base-class cast sometimes fails to preserve null pointer
//
// Version 4.13 introduced a regression causing the front end to fail to
// preserve null pointer values in certain base-class casts.
//
// Previously, the cast "(I*)P()" turned the null address produced by "P()" into
// an offset from a null address.  Now, the null address is preserved.
struct I { I* n; };
struct K { int i; };
struct V: K, I {};
struct M {
   using P = V*;
   constexpr bool f() {
     return (I*)P() == nullptr;
   }
};
static_assert(M().f(), "Unexpected");  // Previously failed.  Now okay.
