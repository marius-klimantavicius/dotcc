//type:fp
//options_all:--c++11
//remark:[4.5] Lowering of value-initialized construction whose constructor is defaulted
// 3/30/12  [EDGcpfe/12806]
//
// Lowering of value-initialized construction whose constructor is defaulted
//
// Lowering now correctly assumes that a defaulted constructor is not
// user-provided, and subsequently uses zero-initialization when
// value-initialization specifies a defaulted constructor.
// --c++11):
typedef __EDG_SIZE_TYPE__ size_t;
extern "C" void *memset(void *s, int c, size_t n);
extern "C" void *malloc(size_t n);
struct X {
  int i;
  X() { i = 1; };
};
struct A {
  X x;
  A() = default;
  int ii;
  void *operator new(size_t sz) {
    void *result = malloc(sz);
    memset(result, 1, sz);
    return result;
  }
};
int main() {
  A *p = new A();
  return p->ii != 0;    // had returned 1, now returns 0
}
