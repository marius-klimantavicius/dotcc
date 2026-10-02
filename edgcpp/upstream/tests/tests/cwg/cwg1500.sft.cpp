//type:fp
//options_all:--c++20 -tused -A
template<typename T>
struct A {
  operator int() { return 0; }

  void f() {
    operator T();
  }
};

int main() {
  A<int> a;
  a.f();
}

//cwg: 1500
//title: Name lookup of dependent conversion function
//meeting: Virtual 11/20*
//edg_status: Passes
