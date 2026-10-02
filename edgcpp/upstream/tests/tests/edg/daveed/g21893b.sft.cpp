//remark:Expl. conv. func. selection rescanning
//options:--c++11;fp


template<typename T> auto f()->decltype(g(T{}).operator T*());
struct S {
  operator S*() const;
};
S g(S);
auto r = f<S>();


