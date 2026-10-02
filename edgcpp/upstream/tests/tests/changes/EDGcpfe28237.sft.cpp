//type:fp
//options_all:--c11 --gnu_version 130200
//remark:[6.8] Statement expressions in file-scope memory
// 6/25/25  [EDGcpfe/28237]
//
// Statement expressions in file-scope memory
//
// This previously resulted in an internal error in add_to_variables_list.  That
// is now fixed.
struct S { char *str; } *sptr;
void g() {
  sizeof(struct {
    _Static_assert(!(__builtin_has_attribute(&({
                                                 typeof(*sptr) *var = 0;
                                                 ((typeof(*(sptr)) *)(var));
                                               })->str,
                                             common)),
                   "unexpected");
  });
}
