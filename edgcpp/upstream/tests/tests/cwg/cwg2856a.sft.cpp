//type:fn
//options_all:--c++23 
  struct A {
     explicit A(int = 10);
     A() = default;   // converting constructor (11.4.8.2 [class.conv.ctor] paragraph 1)
  };
  A b;            // #3
