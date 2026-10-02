//type: fp
//options:  --c++20 --modules
# 0 "./modules/pmf-2_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pmf-2_b.C"


# 1 "./modules/pmf-2.h" 1
template<typename _Tp>
struct remove_reference
{ typedef _Tp FOO; };

template<typename _Tp>
void forward (typename remove_reference<_Tp>::FOO const& __t)
{
}

template<typename _Callable>
void __invoke(_Callable const & __fn)
{
  forward<_Callable const>(__fn);
}

class _State_baseV2
{
public:
  void _M_set_result()
  {
    __invoke (&_State_baseV2::_M_do_set);
  }

  void _M_do_set();
};
# 4 "./modules/pmf-2_b.C" 2
import "pmf-2_a.H";
