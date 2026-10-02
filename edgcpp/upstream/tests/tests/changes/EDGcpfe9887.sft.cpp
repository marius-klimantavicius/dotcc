//type:fp
//remark:[4.1] Array placement new with default arguments lowered incorrectly
// 6/8/09   [EDGcpfe/9887]
//
// Array placement new with default arguments lowered incorrectly
//
// An array placement new call to an operator new[] routine that has one or
// more default arguments was lowered incorrectly (the default arguments
// were not passed to operator new[]).  This problem existed in both ABIs.
// This regression was introduced in 4.0 and is now fixed.
typedef __EDG_SIZE_TYPE__ size_t;
int arg_value = 0;
struct A {
  A() {}
  void* operator new[](size_t size, int x = 57) {
    arg_value = x;
    return (void*)0;
  }
};
int main() {
  A *a = new A[10];
  return (arg_value == 57) ? 0 : 1;
}
