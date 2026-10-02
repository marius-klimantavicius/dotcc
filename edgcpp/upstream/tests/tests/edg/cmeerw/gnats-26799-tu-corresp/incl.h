namespace ns
{
  class C {
  public:
    template<typename, typename>
    struct B;
  };

  template<typename>
  class A {
    template<typename, typename> friend class C::B;
  };
}

namespace ns2
{
  template<int _Idx, typename... _Elements>
  struct _Tuple_impl;

  template<int _Idx>
  struct _Tuple_impl<_Idx>
  {
    template<int, typename...> friend class _Tuple_impl;
  };
}
