//type:fn
//options:--c++20

namespace default_member_init
{
  template<int I = 0>
  struct B
  {
    ~B() { static_assert(I == -1); } // error for I=1,2
  };

  struct C1
  {
    B<1> b1{ };
  };

  C1 c1{ };

  struct C2
  {
    B<2> b2 = { };
  };

  C2 c2{ };
}

namespace member_ctor
{
  template<int I = 0>
  struct B
  {
    B();
    B(int);
    ~B() { static_assert(I == -1); } // error for I=1,2,3,4,5
  };

  struct C1
  {
    B<1> b1{ };
  };

  C1 c1{ };

  struct C2
  {
    B<2> b2 = { };
  };

  C2 c2{ };

  struct C3
  {
    B<3> b3{ 1 };
  };

  C3 c3{ };

  struct C4
  {
    B<4> b4 = { 1 };
  };

  C4 c4{ };

  struct C5
  {
    B<5> b5 = 1;
  };

  C5 c5{ };
}
