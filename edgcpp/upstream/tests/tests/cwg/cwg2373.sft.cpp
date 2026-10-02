//type:rp
//options_all:--c++17 -tused -A

  struct Foo {
    template <typename T>
    static int f(Foo*) { return 0; }

    template <typename T, typename A1>
    int f(const A1&) { return 1; }
  };

  int main() {
    Foo x;
    return x.f<int>(&x);
  } 

//cwg: 2373
//title: Incorrect handling of static member function templates in partial ordering
//meeting: San Diego 11/18
//edg_status: Passes
