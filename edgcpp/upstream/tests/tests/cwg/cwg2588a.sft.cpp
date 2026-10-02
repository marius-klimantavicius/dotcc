//options_all:--c++23 -A
  export module Foo;
  class X {
    friend void f(X); // #1 linkage?
  };
