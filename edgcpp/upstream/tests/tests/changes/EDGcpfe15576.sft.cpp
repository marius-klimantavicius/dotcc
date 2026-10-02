//type:fp
//remark:[4.10] Switch statements controlled by an enum bit field
// 10/22/14 [EDGcpfe/15576]
//
// Switch statements controlled by an enum bit field
//
// The front end previously issued a spurious error on certain case labels in
// switch statements controlled by a bit field of enumeration type.
//
// This is now fixed.  Note that this case was already accepted with a warning in
// GNU C++ mode; that warning is no longer issued.
enum E1 { e1 };
enum E2 { e2 };
struct S { E1 f : 16; } s;
void f() {
  switch (s.f) {
  case e2: ;  // Previously triggered a spurious error indicating that e2
  }           // must be of type E1.
}
