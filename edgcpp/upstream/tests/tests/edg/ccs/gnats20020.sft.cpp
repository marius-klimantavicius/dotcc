//type:cp
//options::--gnu_version 80100:--clang_version 80000
//options_all:--c++11

struct A {
  void end() = delete;
};

struct B {
  void begin() = delete;
};

template<typename T>
int* begin(T&);

template<typename T>
int* end(T&);

void f() {
  for (auto a : A());
  for (auto b : B());
}
