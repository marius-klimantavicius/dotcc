//options_all:-r -x -tused
//options: --strict;cn

struct foo {
      int a;
      foo(int) ;
} ;

struct bar : public foo {
      void baz();
} ;

bar * make_bar() { return new bar; }

