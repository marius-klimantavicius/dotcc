//type:fn
//options:--c++20:--ms_c++20
//options_all:-w

namespace ns
{
  enum E { E1, E2 };
}

namespace using_enumerator_only
{
  struct C
  {
  private:
    using ns::E::E1;
  };

  struct D : C
  {
    static inline auto i = E1;     // inaccessible
  };
}

namespace using_member_and_using_enumerator
{
  struct C
  {
  private:
    using ns::E::E1;

  public:
    using ns::E::E2;
  };

  struct D1 : C
  {
  public:
    using C::E1;                // inaccessible

  private:
    using C::E2;
  };

  struct D2 : D1
  {
    static inline auto i2 = E2; // inaccessible
  };

  auto i = (C::E1,              // inaccessible
            C::E2,
            D1::E2);            // inaccessible
}
