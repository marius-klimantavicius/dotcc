//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
export module quux;
// { dg-module-cmi quux }
import frob;

export int foo (frob *p)
{
  return p->m;
}
