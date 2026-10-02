//type:fn
//options::--gnu_version 80100:--clang_version 80000
//options_all:--c++11

struct C {
  int* begin() = delete;
  int* end() = delete;
};

template<typename T>
int* begin(T&);

template<typename T>
int* end(T&);

void f() {
  for (auto c : C());
}
