//type:fn
//remark:[4.1] Abort on enum definition with global scope qualifier
// 2/17/09  [EDGcpfe/9549]
//
// Abort on enum definition with global scope qualifier
//
// The front end aborted in move_to_end_of_types_list (in il.c) when processing
// an enum definition with a global scope qualifier outside the global scope.
//
// This is now fixed (an error is issued on such cases).
enum E *p;
namespace N {
  enum ::E { e };  // Previously triggered an abort.
}
