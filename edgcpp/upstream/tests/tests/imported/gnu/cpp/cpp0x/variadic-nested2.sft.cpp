//type: fp
//options: --c++11
// PR c++/84839
// { dg-do compile { target c++11 } }

template<typename... T>
struct S {
    using fptr = void(*)(T... x, decltype(x)... y);
};

using F = S<int>::fptr;
