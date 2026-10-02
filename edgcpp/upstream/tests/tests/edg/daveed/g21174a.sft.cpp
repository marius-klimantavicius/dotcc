//remark:Member lookup during member substitution
//options:--gnu=80000 -tused;fp

template <bool b>
struct integral_constant
{
      static constexpr bool value = b;
};
int swap();
struct C
{
    template< typename T >
    static bool test()
    {
        return noexcept( swap() );
    }
    template< typename T >
    static auto test( int ) -> integral_constant<test<T>()>{}
    template< typename > static integral_constant<false> test(...);
};

 
template< typename T >
struct is_nothrow_swappable : decltype( C::test<T>(0) ) {};
 
void foo() {
    is_nothrow_swappable<int&>::value;
}
