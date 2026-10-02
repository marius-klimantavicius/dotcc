//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
// { dg-module-cmi foo:baz }

export module foo:baz;

export int baz ()
{
  return -1;
}
