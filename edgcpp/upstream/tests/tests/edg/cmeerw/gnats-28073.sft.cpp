//type:fp
//options:--c++11 -A:--c++20 -A:--c++11 --gn 150100:--c++20 --gn 150100:--c++20 --clang_version 200100;fn:--ms_c++20 --microsoft_version 1944;fn

namespace minimal
{
  template<typename T> struct C;
  template<typename T> using A = C<T>;
  template<typename T, template<typename> class> struct B;
  template<typename T> struct B<T, C> { };
  B<int, A> b;
}

namespace alias_in_partial_spec
{
  template<typename T>
  struct C;

  template<typename T>
  using A = C<T>;

  template<typename T, template<typename> class>
  struct B;

  template<typename T>
  struct B<T, A> { };

  B<int, A> b1;
  B<int, C> b2;
}

namespace base_template_in_partial_spec
{
  template<typename T>
  struct C;

  template<typename T>
  using A = C<T>;

  template<typename, template<typename> class>
  struct B;

  template<typename T>
  struct B<T, C> { };

  B<int, A> b1;
  B<int, C> b2;
}
