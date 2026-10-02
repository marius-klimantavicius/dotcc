//type: fp
//options:  --c++20 --modules
// { dg-additional-options {-fmodules-ts} }
export module Foo;
// { dg-module-cmi {Foo} }
import "pr98741_a.H";
