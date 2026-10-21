using A=const char[3]; void f(A); void f(const char*);
template<class A,class B> constexpr bool same=false;
template<class A> constexpr bool same<A,A> =true;
static_assert(same<decltype(&f),void(*)(const char*)>);
