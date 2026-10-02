//type:fp
//options:--c++14:--c++14 --gn 150100:--c++ --gn 40200:--c++ --gn 40300
//options_all:-w

namespace minimal
{
#if __cplusplus >= 201403
  template<int> bool b = false;
  template<int I> int v = b<I>?1:2;
#endif
}

namespace other_tests
{
#if defined(__GNUC__) && (__GNUC__ < 4 || (__GNUC__ == 4 && __GNUC_MINOR__ < 3))
  int i = 1 >? 2;
  int j = 2 <? 3;
#else
  template<int> void f();
  int v = &f<1>?1:2;
#endif
}
