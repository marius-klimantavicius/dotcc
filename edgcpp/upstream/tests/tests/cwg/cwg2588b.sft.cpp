//options_all:--c++23 -A
  export module Foo;
  export class Y;
  // maybe many lines later, or even a different partition of Foo
  class Y {
    friend void f(Y); // #2 linkage?
  };

