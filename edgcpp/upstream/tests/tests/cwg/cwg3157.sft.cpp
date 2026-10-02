//type:fp
//options: -A --c++20

struct A {};
struct T { T(); };

void *operator new[](decltype(sizeof(0)), A &);  // #1

template <int = 0>
void operator delete[](void *, A &);  // #2

A al;

void f() {
  // The placement array new-expression selects #1; because T's constructor
  // can throw, a matching placement operator delete[] is needed. Deducing the
  // function template #2 to match the placement operator new[] relies on
  // [temp.deduct.decl] handling array-new (CWG 3157).
  T *p = new (al) T[2];
  (void)p;
}

//cwg: 3157
//title: Missing handling of operator new[] for deallocation function template matching
//meeting: Croydon 3/26
//edg_status: Passes
