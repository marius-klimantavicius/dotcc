//type:fp
//options_all:--c++20 -tused -A
template<class T, class... Args>
void foo()
{
  static constexpr T t(Args{}...);
}
int main()
{
 foo<int[1000]>();
}

//cwg: 1677
//title: Constant initialization via aggregate initialization
//meeting: Kona 2/17
//edg_status: Passes
