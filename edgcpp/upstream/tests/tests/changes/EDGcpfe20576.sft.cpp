//type:fp
//options_all:--c++14
//remark:[5.1] Abort on constant address representing "one past" a non-array union object
// 12/7/18  [EDGcpfe/20576]
//
// Abort on constant address representing "one past" a non-array union object
//
// The front end previously aborted (in finalize_subobject_path, interpret.c) with
// an internal error when constexpr evaluation produced a result with an address
// one past a union object (that is not an array element).
//
// This regression (introduced in version 5.0 by the changes for EDGcpfe/17710) is
// now fixed.
union U { int x; } x;
constexpr U* f() { return &x+1; }
U *p = f();  // Aborted in 5.0
