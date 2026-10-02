//remark:Deduction guides and typerefs
//options:--c++17;fn:--c++17 --clang;fn:--c++17 --g++;fp:--c++17 --microsoft;fp

template <class T1, class T2>
class C {};

// WORKS: Classic template deduction
template <class T>
C(T) -> C<T, T>;


// ---- Simple using case:

using SpecializationClass = C<int, int>;

// DOES NOT WORK
template <class T>
C(T) -> SpecializationClass;


// ---- Simple template using case:

template <class T>
using AliasClass = C<T, T>;

// DOES NOT WORK
template <class T>
C(T) -> AliasClass<T>;


// ---- Our effective case:

template <template<class...> class Class, class T>
using ProxyClass = Class<T, T>;

// DOES NOT WORK
template <class T>
C(T) -> ProxyClass<C, T>;
