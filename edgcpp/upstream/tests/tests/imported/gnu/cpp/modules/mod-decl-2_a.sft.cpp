//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module bob;
// { dg-module-cmi "bob" }
export void Foo ();
export 
{
  void Bar ();
}
