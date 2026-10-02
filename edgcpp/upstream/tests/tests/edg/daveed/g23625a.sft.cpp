//remark:Fold-expressions in alias templates
//options:--c++17 -tused;fp

template<bool B> using I = int;
template <class... Ts> using AllC = I<(Ts::c && ...)>;
template <class... i> struct X {
  using Type = AllC<i...>;
};

//AllC<> *p;  // Okay
X<>::Type *x;  // Error
