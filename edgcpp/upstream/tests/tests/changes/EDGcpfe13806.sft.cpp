//type:fp
//remark:[4.6] Abort on local anonymous union introducing a new type name
// 3/26/13  [EDGcpfe/13806]
//
// Abort on local anonymous union introducing a new type name
//
// In some unusual cases, the front end aborted with an internal error in
// cancel_name_collision_discriminator when encountering a local anonymous union
// declaring a member that itself introduces a new type name.
//
// (the name "B" matters here, because the failure mechanism involves a hash
// table collision).  This is now fixed.
void f() {
  union {
    struct B *x;
  };  // Previously triggered an internal error.
}
