//type:fp
//options_all:--c++17
//remark:[5.0] Segfault in is_nothrow_spec on use of typedef for function
// 4/6/18   [EDGcpfe/19520]
//
// Segfault in is_nothrow_spec on use of typedef for function
//
// In configurations that do lowering, certain uses of typedefs for function
// types had resulted in a segmentation fault in is_nothrow_spec.
// (with --c++17):
void f(int) noexcept {}
typedef void (FP)(int);
int main() {
  try {
    throw f;
  }
  catch (FP) {
  }
}
