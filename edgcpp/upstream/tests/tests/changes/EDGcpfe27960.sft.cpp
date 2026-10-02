//type:fp
//options_all:--c++11
//remark:[6.8] Internal error in find_allocated_name_reference
// 2/25/25  [EDGcpfe/27960,EDGcpfe/27983]
//
// Internal error in find_allocated_name_reference
//
// In configurations with DEFAULT_RECORD_FORM_OF_NAME_REFERENCE set to TRUE, the
// changes for EDGcpfe/27200 (in version 6.7) could result in an internal error
// due to a failed assertion in find_allocated_name_reference.
// --c++11:
using P = struct { int i; };
void f(P *p) {
  p->~P();  // Previously triggered an internal error in some configurations.
}           // Now okay.
