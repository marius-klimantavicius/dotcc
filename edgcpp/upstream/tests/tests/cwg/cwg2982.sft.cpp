//type:fn
//options_all:--c++20 -A
template<typename T, typename U> concept C = true;
template<typename T> C<T> auto f() { return 0; }
template C<int> auto f();

//cwg: 2982
//title: Deduction in type-constraints
//meeting: Sofia 6/25
//edg_status: Passes
