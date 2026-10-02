//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module foo;
// { dg-module-cmi "foo" }

export int bar ()
{
  return 1;
}

export int baz ()
{
  return 2;
}

export int quux ()
{
  return 3;
}

