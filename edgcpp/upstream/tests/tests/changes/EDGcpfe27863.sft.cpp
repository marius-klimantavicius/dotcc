//type:fp
//options_all:--ms_extensions --ms_extensions --ms_extensions
//remark:Segfault in lower_delete with delete[] with --ms_extensions
// 2/3/26   [EDGcpfe/27863,EDGcpfe/28427]
//
// Segfault in lower_delete with delete[] with --ms_extensions
//
// A segmentation fault could occur in lower_delete when --ms_extensions is used
// with certain delete[] operations.
void f(int *ptr) {
  delete[] ptr;
}
