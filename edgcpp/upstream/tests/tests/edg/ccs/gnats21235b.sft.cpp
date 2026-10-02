//type:cp
//options::--gnu_version 70000
//options_all:--c++17 -tused

template<class Ty>
struct A
{
  constexpr A(A&& same_parameter_name) noexcept(true) = default;
  constexpr operator bool() const { return false; }
};


bool foo(const A<int>& lhs, int same_parameter_name)
{
  return (!lhs);
}
