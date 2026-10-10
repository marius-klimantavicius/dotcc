// Preserve the operand category of an unevaluated sizeof...(pack).
namespace original_pack_sizes {
template<class... T> struct Types { static int (*value())[sizeof...(T)+1]; };
template<int... N> struct Values { static int (*value())[sizeof...(N)+1]; };
template<template<class> class... T> struct Templates { static int (*value())[sizeof...(T)+1]; };
template<class> struct Box {};
template<class... T> auto function(T... values) -> int (*)[sizeof...(values)+1];
template<class T> concept Chosen = requires {
    Types<int,long>::value(); Values<1,2,3>::value(); Templates<>::value(); function(1,2);
};
template<Chosen T> struct Keep { T member = {}; };
static_assert(sizeof(*Types<int,long>::value()) == 3*sizeof(int));
static_assert(sizeof(*Values<1,2,3>::value()) == 4*sizeof(int));
static_assert(sizeof(*Templates<>::value()) == 1*sizeof(int));
static_assert(sizeof(*function(1,2)) == 3*sizeof(int));
}
int main() { return 0; }
