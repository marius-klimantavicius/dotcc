//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
// From Andrew Sutton

export module foo;
// { dg-module-cmi foo }
export class A {
  friend class B;
};
