//type:fn
//options:--c++11:--c++20:--ms_c++20

class C
{
  friend class B;

  enum E { E1 };
};

struct B
{
  using E = C::E;

  enum G { G1 };
};

struct D : public B
{
  E f()
  {
    return E::E1;
  }

  E g()
  {
    return C::E1;               // "E1" inaccessible
  }

  E h()
  {
    return C::E::E1;            // "E" inaccessible
  }

private:
  using B::G;
};

struct DD : D
{
  B::G f()
  {
    return G1;
  }

  B::G g()
  {
    return G::G1;               // "G" inaccessible
  }

  B::G h()
  {
    return B::G::G1;
  }

  B::G i()
  {
    return B::G1;
  }
};
