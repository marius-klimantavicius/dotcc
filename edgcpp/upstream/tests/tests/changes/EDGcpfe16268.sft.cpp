//type:fp
//options_all:--c++20 --microsoft
//remark:[6.1] Microsoft compatibility: additional encoding-prefix string literal operators
// 4/2/20   [EDGcpfe/16268]
//
// Microsoft compatibility: additional encoding-prefix string literal operators
//
// As described in the entries of 2/18/98 and 5/12/04, the front end supports
// several Microsoft extensions for producing wide string literals: prefixing
// the stringize operator # with L, concatenating L with a function-name token
// like __FUNCTION__, and the built-in operator __LPREFIX.  These extensions
// have now been expanded to include parallel mechanisms for producing string
// literals with the encoding-prefixes U, u, and u8 as both raw and non-raw
// strings.  In particular, __uPREFIX, __UPREFIX, and __lPREFIX add the
// encoding-prefixes u, U, and u8, respectively, to their string operand; the
// concatenation and stringize operations use the encoding-prefixes directly.
#define _U(x) U##x
#define U(x) _U(x)
#define uR(x) uR#x
void f() {
  auto a = U(__FUNCTION__);  // equivalent to U"f"
  auto b = uR(xx(abc)xx);    // equivalent to uR"xx(abc)xx", i.e., u"abc"
  auto c = __lPREFIX("z");   // equivalent to u8"z"
}
