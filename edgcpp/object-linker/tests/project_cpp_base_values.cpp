// Nonvirtual complete/base views and copies, with no RTTI runtime imports.
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
  return 0;
}
