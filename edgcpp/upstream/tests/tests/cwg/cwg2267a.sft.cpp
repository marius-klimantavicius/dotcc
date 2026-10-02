//type: fp
//options: 
//options_all: -A --c++20 -tused -e 200 --no_wrap

  struct A { } a;
  struct B { explicit B(const A&); };
  B b1(a);            // #1, ok 
