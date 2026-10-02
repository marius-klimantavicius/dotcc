//type: fp
//options: 
# 1 "SemaCXX/warn-new-overaligned-3.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-new-overaligned-3.cpp" 2







# 1 "SemaCXX/Inputs/warn-new-overaligned-3.h" 1
# 2 "SemaCXX/Inputs/warn-new-overaligned-3.h" 3




void* operator new(unsigned long) {
  return 0;
}
void* operator new[](unsigned long) {
  return 0;
}

void* operator new(unsigned long, void *) {
  return 0;
}

void* operator new[](unsigned long, void *) {
  return 0;
}
# 9 "SemaCXX/warn-new-overaligned-3.cpp" 2

namespace test1 {
struct Test {
  template <typename T>
  struct SeparateCacheLines {
    T data;
  } __attribute__((aligned(256)));

  SeparateCacheLines<int> high_contention_data[10];
};

void helper() {
  Test t;
  new Test;
  new Test[10];
}
}

namespace test2 {
struct helper { int i __attribute__((aligned(256))); };

struct Placement {
  Placement() {
    new (d) helper();
  }
  helper *d;
};
}
