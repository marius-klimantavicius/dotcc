//options_all:--c++23 -A
namespace N
{
    template<typename T>
    struct A;
}

template<>
struct N::A<int>; // #1

template<typename T>
struct N::A<T*>; // #2

//cwg: 2874
//title: Qualified declarations of partial specializations
//meeting: St Louis 6/24
//edg_status: Passes
