//remark:Deducing bool templ param from noexcept
//options:--gnu=80400 --c++17;fp

void f() noexcept;
 
template<typename> struct is_function {
   static constexpr bool value = false;
};
 
template<typename _Res, typename... _ArgTypes , bool _NE>
struct is_function<_Res(_ArgTypes...) noexcept (_NE)>
{ static constexpr bool value = true; };
 
static_assert(is_function<decltype(f)>::value,"assertion failure");
