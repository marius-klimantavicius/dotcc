//type:fp
//options_all:--c++20 -tused -A

template<class T, class D = int>
struct S {
T data;
};
template<class U>
explicit (true) S(U) -> S<typename U::type>;

//cwg: 2422
//title: Incorrect grammar for deduction-guide
//meeting: Belfast 11/19
//edg_status: Passes
