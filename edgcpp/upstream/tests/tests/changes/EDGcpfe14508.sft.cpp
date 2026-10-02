//type:fp
//options_all:--g++
//remark:[4.9] GNU C++ compatibility: Passing a packed array member to reference to const
// 1/2/14   [EDGcpfe/14508]
//
// GNU C++ compatibility: Passing a packed array member to reference to const
// (IL CHANGE)
//
// In g++ modes, when passing a field of a packed type to a parameter of
// reference to const type, the front end normally copies the field to a
// suitably aligned temporary, unless the field is of a non-POD class type (see
// the Changes entry for EDGcpfe/8366).  Previously, however, this failed when
// the field was an array: A spurious conversion error was issued.
//
// This is now fixed.  The fix involves slight IL CHANGE: Dynamic init entries
// of kind dik_bitwise_copy now sometimes include a pointer to a source
// expression (previously, all such entries had an implicit source).
//
// In addition to the fix above, the copy to an aligned temporary is now
// inhibited if the destination type is single-byte aligned (the copy is
// unneeded in such cases since the member itself is not unaligned).
void g(const int (&p)[1]);
struct  __attribute__ ((packed)) Packed {
  char c;
  int i[1];
};
Packed p = {0, 1};
int main () {
  g(p.i);  // Previously an error.  Now accepted.
}
