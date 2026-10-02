//type:fp
//options_all:--c++17 -tused -A
  struct Params { int a; int b; };
  class Foo {
  public:
    Foo(Params);
  };
  Foo foo{{1, 2}};
