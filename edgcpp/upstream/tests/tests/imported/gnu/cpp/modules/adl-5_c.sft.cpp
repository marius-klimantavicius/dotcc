//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

module hidden;
import foo;

int frob ()
{
  X x (2);

  if (frob (x) != 2)
    return 1;

  if (TPL (x) != 2)
    return 2;

  return 0;
}
