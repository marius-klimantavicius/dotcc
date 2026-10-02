//type:fn
//options_all:--c++23 -tused -A
  consteval int f(int);
  struct S {
   int x = f(0);
   S() = default;
  };

  int main() {
    S s;     
  }

//cwg: 2760
//title: Defaulted constructor that is an immediate function
//meeting: Kona 11/23
//edg_status: Passes
