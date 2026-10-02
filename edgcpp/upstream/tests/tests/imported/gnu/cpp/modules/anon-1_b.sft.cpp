//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module namer;
// { dg-module-cmi namer }

import anon;

export inline int &get_int (foo &obj)
{
  return obj.i;
}

export inline float &get_float (foo &obj)
{
  return obj.f;
}


