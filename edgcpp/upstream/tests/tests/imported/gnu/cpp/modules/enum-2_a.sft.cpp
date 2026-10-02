//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module foo;
// { dg-module-cmi foo }

export struct X 
{
  enum q
  {
    frob
  };
};

export template <int I> struct TPL
{
  enum p
  {
    u,
    v = I
  };
};
