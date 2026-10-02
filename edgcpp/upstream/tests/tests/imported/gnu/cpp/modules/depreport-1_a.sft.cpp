//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module Foo;
// { dg-module-cmi Foo }

export class Base
{
public:
  int m;
};
