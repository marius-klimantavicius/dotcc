//type:fn
//options:--c++20

template<typename C>
struct A {
};

template<typename AT>
using A_alias = typename A<AT>::X; // there should be an error here

struct B : A_alias<int> { // there should not be an error on this base

};
