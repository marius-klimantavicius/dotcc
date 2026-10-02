//type:fp
//options:--c++11 -A:--c++11 --gn 130200:--c++17 --gn 130200

namespace minimal
{
  template<int> struct D {
    template<typename T, typename ... TT> using B = T;
  };
  template<int I, typename ... TT> using B = typename D<I>::template B<TT ...>;
  template<typename> struct X;
  template<typename ... TT> struct C {
    using BB = B<0, TT ..., void>;
    X<BB> *x;
  };
}

#if __cpp_variable_templates
namespace pr
{
  struct a {
    template <int> struct b {
      template <int, typename c, typename...> using d = c;
    };
    template <int e, typename... f> using g = typename b<e>::template d<e, f...>;
  };
  template <int e, typename... f> using g = a::g<e, f...>;
  template <typename> bool h;
  template <typename... f> struct i {
    using j = g<0, f..., void>;
    bool k = h<j>;
  };
}

namespace somewhat_simplified
{
  template<int> struct b
  {
    template<int, typename c, typename...> using d = c;
  };

  template<int e, typename ... f> using g = typename b<e>::template d<e, f...>;
  template<typename> bool h;

  template<typename... f> struct i
  {
    using j = g<0, f..., void>;
    bool k = h<j>;
    bool l = h<g<0, f..., void>>;
  };
}
#endif
