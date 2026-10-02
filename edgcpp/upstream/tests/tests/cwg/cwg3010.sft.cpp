//type:fn
//options: -A --c++20

void *operator new(decltype(sizeof 0), void *p) { return p; }

struct A {
  int n;
};

struct B : A {
  int m;
};

constexpr int f() {
  B b = {{0}, 0};
  A *p = &b;
  new (p) A{1};
  return p->n;
}

constexpr int k = f();  // error

//cwg: 3010
//title: constexpr placement-new should require transparent replaceability
//meeting: Croydon 3/26
//edg_status: Passes
