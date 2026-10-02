//type: fn
//options: --c++20
// PR c++/91319
// { dg-do compile { target c++20 } }

struct X {
    explicit X() { }
};

struct Aggr {
    X x;
};

Aggr
f ()
{
  return Aggr{.x{}};
}

Aggr
f2 ()
{
  return Aggr{.x = {}}; // { dg-error "explicit constructor" }
}
