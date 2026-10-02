#ifndef DOT_GEN_MULTI_TU_SHARED_H
#define DOT_GEN_MULTI_TU_SHARED_H

inline int& counter() { static int value = 3; return value; }
template<class T> inline T twice(T value) { return value + value; }

struct Base {
  int n;
  Base(int value) : n(value) {}
  virtual int get() { return n; }
  virtual ~Base() {}
};
struct Derived : Base {
  Derived(int value) : Base(value) {}
  int get() { return n + 2; }
};

extern int initialized;
int* address_other();
int call_other();
void throw_other();

#endif
