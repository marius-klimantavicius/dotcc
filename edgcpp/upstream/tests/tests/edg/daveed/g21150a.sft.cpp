//remark:Substitution of decltype
//options:--c++17;fp

template <class> void foo();
template <typename T> using X = decltype((foo<T>));
template <typename T> struct A {};
template <typename T> A<X<T>> bar();
auto a = bar<int>();

