//remark:Deduction guides and typerefs
//options:--c++17;fn:--c++17 --clang;fn:--c++17 --g++;fp:--c++17 --microsoft;fp

template <typename...> struct a;
template <typename> using b = a<>;
template <typename c> a()->b<c>;
