// Nested discarded initialization must not invent a C++ record copy.
extern "C" void* malloc(unsigned long);
extern "C" void free(void*);
extern "C" int putchar(int);
void* operator new(unsigned long size) { auto p=malloc(size); if(!p)throw 7; return p; }
void operator delete(void* p) noexcept { free(p); }
void operator delete(void* p,unsigned long) noexcept { free(p); }
struct Value {
  int* pointer;
  constexpr Value() : pointer(nullptr) {}
  Value(const Value&)=delete;
  ~Value() { putchar('V'); }
};
struct Outer { Value value; constexpr Outer() = default; ~Outer() { putchar('O'); } };
Outer* make(){return new Outer;}
struct Iterator {
  int index;
  Iterator(int start) : index(start) {}
  Iterator(const Iterator&) = delete;
  Iterator& operator++() { ++index; return *this; }
  bool operator!=(const Iterator& other) const { return index!=other.index; }
  int operator*() const { return index; }
  ~Iterator() { putchar('I'); }
};
struct Range { Iterator begin() const { return Iterator(0); } Iterator end() const { return Iterator(3); } };
int main() {
  auto p=make();
  if(p->value.pointer)return 1;
  delete p;
  int sum=0;
  for(int current:Range{}) sum+=current;
  return sum==3?0:2;
}
