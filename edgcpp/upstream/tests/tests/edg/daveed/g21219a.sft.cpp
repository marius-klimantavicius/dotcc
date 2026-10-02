//remark:C++17 exception specs and explicit specialization
//options:--c++17;fp:--c++17 --clang;fp

template <typename T>
struct some_type_trait { static constexpr bool value = true; };

template<class _Ty>
void swap2(_Ty& _Left, _Ty& _Right) noexcept(some_type_trait<_Ty>::value);

template<>
void swap2(int& a1, int& a2) noexcept(some_type_trait<int>::value)
{ /* Empty */ }
