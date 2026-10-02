//remark:Singleton braced CTAD
//options:--c++17;fn:--c++17 -DPOS;fp

#include <initializer_list>

template <typename T, typename U> struct is_same { static constexpr bool value = false; };
template <typename T> struct is_same<T, T> { static constexpr bool value = true; };

template<typename T> struct X
{
  X(std::initializer_list<T>);
};

struct Y : X<int> {};
struct Z : X<int>, X<float> {};

X<int> xi = {0};
X xxi = {xi};   //clang and g++ deduce xxi as X<int>
static_assert(is_same<decltype(xxi), X<int> >::value, ""); // -- SHOULD ACCEPT?

Y y {{0}};
X xy {y};       //clang and g++ deduce xy as X<int>
static_assert(is_same<decltype(xy), X<int> >::value, "");

#ifndef POS
Z z = {{0}, {0.0f}};
// It's ambiguous between X<int> and X<float>.
X xz = {z};     //clang rejects, g++ and EDG accept -- SHOULD REJECT?
#endif
