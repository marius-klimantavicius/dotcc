//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -fmodule-mapper=[srcdir]/ben-1.map" }
// { dg-additional-files ben-1.map }

export module module;
// { dg-module-cmi =module.mod }
export import :import;

export int c ()
{
  return b ();
}
