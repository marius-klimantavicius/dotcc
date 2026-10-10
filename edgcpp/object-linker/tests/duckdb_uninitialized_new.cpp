// A defaulted trivial constructor may leave a frontend rollback declaration
// with no new-initializer. Allocation failure and a later expression failure
// must not call placement delete; a throwing nontrivial constructor must.
extern "C" int putchar(int);
using size_t = decltype(sizeof(0));
static int allocations, releases, fail_allocation, fail_constructor;
void* operator new(size_t, void* storage) {
  ++allocations;
  if (fail_allocation) throw 1;
  return storage;
}
void operator delete(void*, void*) noexcept { ++releases; }
struct Trivial { Trivial() = default; int value; };
struct Constructed {
  int value;
  Constructed() { if (fail_constructor) throw 2; value=31; }
};
int consume(Trivial* value) { value->value=17; throw 3; }
int main() {
  alignas(Trivial) unsigned char first[sizeof(Trivial)];
  alignas(Constructed) unsigned char second[sizeof(Constructed)];
  auto value=new(first) Trivial;
  value->value=42;
  if(value->value!=42 || allocations!=1 || releases) return 1;
  putchar('A');
  fail_allocation=1;
  try { (void)new(first) Trivial; return 2; } catch(int value) { if(value!=1 || releases) return 3; }
  fail_allocation=0;
  putchar('B');
  try { consume(new(first) Trivial); return 4; } catch(int value) { if(value!=3 || releases) return 5; }
  putchar('C');
  fail_constructor=1;
  try { (void)new(second) Constructed; return 6; } catch(int value) { if(value!=2 || releases!=1) return 7; }
  fail_constructor=0;
  auto constructed=new(second) Constructed;
  if(constructed->value!=31 || allocations!=5 || releases!=1) return 8;
  putchar('D');
  return 0;
}
