//type: fp
//options:  --c++20 --modules
# 0 "./modules/inh-tmpl-ctor-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/inh-tmpl-ctor-1_b.C"


# 1 "./modules/inh-tmpl-ctor-1.h" 1

template <typename _Tp>
struct Base
{

  template<typename _Del> Base(_Tp *__p, _Del __d);
};

template <typename _Tp, typename _Dp>
struct Derived : Base<_Tp>
{

  using Base<_Tp>::Base;
};

template <typename _Tp>
class unique_ptr
{
  Derived<_Tp, int> _M_t;

public:

  template<typename _Up> unique_ptr(unique_ptr<_Up>&& __u) noexcept
    : _M_t ((_Tp *)0, 1) { }
};

struct ResultBase { };
struct ResultDerived : ResultBase { };

void Frob (unique_ptr<ResultBase> &&__res) ;

inline void X (unique_ptr<ResultDerived> &parm)
{
  Frob (static_cast <unique_ptr<ResultDerived> &&> (parm));
}
# 4 "./modules/inh-tmpl-ctor-1_b.C" 2
import "inh-tmpl-ctor-1_a.H";
