//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module foo;
// { dg-module-cmi foo }

namespace foo {

  export int frob (int i)
  {
    return i;
  }

}
