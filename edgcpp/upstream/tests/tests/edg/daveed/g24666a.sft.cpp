//remark:concept-id substitution
//options:--c++20;fp:--c++20 --gnu=100100;fp:--c++20 --microsoft_v=1930;fp

template<typename> struct X {
    static const bool Val = false;
};
template<typename T> concept C = X<T>::Val;
static_assert(!C<int>);

