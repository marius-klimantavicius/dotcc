//tpye:fp
//options:--c++03 -A;fn:--c++11
//options_all:-w

  template<typename>
  struct S {};
  ::template S<void> s;


template<typename T>
struct C
{
  struct N
  { };
};

namespace ns
{
  template<typename T>
  struct C
  {
    struct N
    { };
  };
}

::C<void> c1;
::template C<void> c2;
typename ::C<void> c3;
typename ::template C<void> c4;

ns::C<void> c5;
ns::template C<void> c6;
typename ns::C<void> c7;
typename ns::template C<void> c8;
