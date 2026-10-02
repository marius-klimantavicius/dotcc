//type:fp
//options_all:--c++17 -tused -A
template<typename T> struct D : T::template B<int>::template C<int> {};

//cwg: 1710
//title: Missing template keyword in class-or-decltype
//meeting: Kona 2/17
//edg_status: Passes
