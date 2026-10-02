//type: fp
//options:  --c++20 --modules
# 0 "./modules/pr106761_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pr106761_b.C"



# 1 "./modules/pr106761.h" 1


template<class...>
struct __and_;

template<class, class>
struct is_convertible;

template<class... Ts>
struct _TupleConstraints {
  template<class... Us>
  using __constructible = __and_<is_convertible<Ts, Us>...>;
};

template<class... Ts>
struct tuple {
  template<class... Us>
  using __constructible
    = typename _TupleConstraints<Ts...>::template __constructible<Us...>;
};

inline tuple<int, int> t;
# 5 "./modules/pr106761_b.C" 2
import "pr106761_a.H";

tuple<int, int> u = t;
