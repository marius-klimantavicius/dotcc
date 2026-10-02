
//options_all:--c++23 -tused -A
template<class T> concept True = true;

template<class T> struct X {
  template<class U> requires True<T> X(T, U(&)[3]);
};
template<typename T, typename U> X(T, U(&)[3]) -> X<T>;
int arr3[3];
X z(3, arr3);     // #1

//cwg: 2628
//title: Implicit deduction guides should propagate constraints
//meeting: Kona 11/23
//edg_status: Passes
