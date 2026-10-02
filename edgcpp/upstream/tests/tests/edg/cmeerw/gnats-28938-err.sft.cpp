//type:fn
//options:--c++14


namespace invalid_non_type_argument
{
  int i;

  constexpr void * const p = &i;
  template<void *> int v;
  template int v<p>;            // error
}

namespace incompatible_templ_templ_parameters
{
  template<int>
  struct X { };

  template<typename T, template<T> class C>
  int v;

  int i = v<long, X>;           // error
}

namespace error_partial_spec
{
  template<typename T, typename U> int v = 0;
  template<typename T> T v<T, 1> = 1; // error
}

namespace conversions_from_error_marker
{
  template<typename T, int &I> T x = 0;
  bool b = x<int, true> == 0;   // error
  int i = x<int, true>;         // error
}
