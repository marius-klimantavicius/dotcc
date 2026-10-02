//type:fp
//options_all:--microsoft
//remark:[4.9] Microsoft compatibility: Bitwise copying and __unaligned
// 12/18/13 [EDGcpfe/13751,EDGcpfe/14719]
//
// Microsoft compatibility: Bitwise copying and __unaligned
//
// The changes for EDGcpfe/9157 included permitting conversions that drop the
// __unaligned specifier.  However, the case where the conversion amounts to a
// bitwise class copy was accidentally omitted, and still caused spurious errors.
//
// This is now fixed.
struct S { int x; };
void g(S __unaligned *p) {
  S x = *p;  // Previously an error; now okay.
}
