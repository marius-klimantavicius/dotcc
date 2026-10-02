//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap

void f2() {
    void g6(int = ([x=1] { return x; })());     // OK
  }

//cwg: 2358
//title: Explicit capture of value
//meeting: Kona 02/19
//edg_status: Passes
