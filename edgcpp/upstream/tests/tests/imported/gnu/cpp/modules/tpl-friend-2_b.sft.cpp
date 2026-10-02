//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
module foo;

void foo (int x, void *p)
{
  auto *obj = reinterpret_cast<TPL<int> *> (p);

  obj->member = x;
}
