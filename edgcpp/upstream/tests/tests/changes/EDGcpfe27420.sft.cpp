//type:fp
//options_all:--c++20 --g++
//remark:[6.7] GCC compatibility: class template argument deduction for alias templates
// 7/3/24   [EDGcpfe/27420]
//
// GCC compatibility: class template argument deduction for alias templates
//
// When using the deduction guide to deduce the type of "a", the C++ Standard
// requires the template arguments of the alias template to be deducible from the
// return type of that deduction guide.  GCC considers the template arguments to
// be deducible, even though substituting the deduced arguments into the alias
// template yields a different type.  The front end now emulates that behavior.
template<int, int> struct D;
template<typename> struct C {};
C() -> C<D<1, 2>>;
template<int I> using A = C<D<I, +I>>;
A a{};  // Now accepted in GNU mode.
