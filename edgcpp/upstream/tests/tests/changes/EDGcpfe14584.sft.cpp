//type:fp
//remark:[4.9] Objectless reference to member of nonstandard anonymous struct
// 10/22/13 [EDGcpfe/14584]
//
// Objectless reference to member of nonstandard anonymous struct
//
// In configurations in which ALLOW_NONSTANDARD_ANONYMOUS_UNIONS is TRUE, an
// objectless reference to a member of a nonstandard anonymous struct
// subobject resulted in an abort (a failed assertion in scan_identifier).
// This is now fixed.
struct A {
  union {
    struct {
      int m;
    };
  };
};
int i = sizeof(A::m);  // Previously aborted
