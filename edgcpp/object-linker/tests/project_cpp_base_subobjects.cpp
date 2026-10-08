// Exact tail views, empty-base address identity, ordinary derived value copies,
// and real generated constructor/destructor entries. Exceptions are disabled.
static int destruction;
static int deletion;
void operator delete(void*) noexcept { ++deletion; }
void operator delete(void*, __SIZE_TYPE__) noexcept { ++deletion; }
void* operator new(__SIZE_TYPE__, void* p) noexcept { return p; }
struct Empty {};
struct Tail {
  int value;
  char mark;
  Tail(int n) : value(n), mark(7) {}
};
struct Derived : Empty, Tail {
  char reused;
  Derived(int n) : Tail(n), reused(19) {}
};
static Derived roundtrip(Derived value) { return value; }
struct Left { int left; Left() : left(11) {} };
struct Right { int right; Right() : right(13) {} };
struct Multiple : Left, Right { int last; Multiple() : last(17) {} };
struct VirtualBase {
  int value;
  VirtualBase(int n) : value(n) {}
  virtual ~VirtualBase() { destruction += value; }
};
struct VirtualDerived : VirtualBase {
  int extra;
  VirtualDerived(int n, int m) : VirtualBase(n), extra(m) {}
  ~VirtualDerived() override { destruction += extra; }
};
static void destroy(VirtualBase* value) { delete value; }
int main() {
  Derived original(23);
  if ((void*)(Empty*)&original != (void*)&original) return 1;
  if (original.value != 23 || original.mark != 7 || original.reused != 19) return 2;
  Derived copied = roundtrip(original);
  copied.value = 29;
  if (original.value != 23 || copied.value != 29 || copied.mark != 7 || copied.reused != 19) return 3;
  original = copied;
  if (original.value != 29 || original.reused != 19) return 4;
  Tail replacement(41);
  static_cast<Tail&>(original) = replacement;
  if (original.value != 41 || original.mark != 7 || original.reused != 19) return 7;
  Multiple multi;
  if (multi.left != 11 || multi.right != 13 || multi.last != 17) return 5;
  alignas(16) unsigned char buffer[sizeof(VirtualDerived)];
  VirtualBase* allocated = new (buffer) VirtualDerived(31,37);
  destroy(allocated);
  destroy(nullptr);
  if (destruction != 68 || deletion != 1) return 6;
  return 0;
}
