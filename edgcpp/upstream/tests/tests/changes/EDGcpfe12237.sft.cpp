//type:fp
//options_all:--microsoft
//remark:[4.5] Microsoft compatibility: Scope of out-of-class member enumerator constants
// 2/15/12  [EDGcpfe/12237,EDGcpfe/12578]
//
// Microsoft compatibility: Scope of out-of-class member enumerator constants
//
// In Microsoft mode, the front end accepts out-of-class definitions of member
// enum types (see Changes entry of 11/30/04).  Previously, however, the
// enumerator constants were not visible in the class scope; now, they are.
struct S { enum E; };
enum S::E { e1, e2 };
S::E x = S::e1;  // Previously an error.  Now accepted.
