//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename> concept X = true;
  template<typename>
  struct C {
    template<X> struct B {};
    B<int> b;
  };
}

namespace member_with_constrained_params
{
  template<typename> concept X = true;

  template<typename T>
  struct C
  {
    template<X> struct B {};
    B<int> b1;
    B<T> b2;
  };

  template<typename T>
  struct C<T *>
  {
    template<X> struct B {};
    B<int> b1;
    B<T> b2;
  };

  template<>
  struct C<int *>
  {
    template<X> struct B {};
    B<int> b1;
  };

  template struct C<void>;
  template struct C<char *>;
}

namespace non_member_with_constrained_params
{
  template<typename> concept X = true;
  template<X> struct B {};

  template<typename T>
  struct C
  {
    B<int> b1;
    B<T> b2;
  };

  template<typename T>
  struct C<T *>
  {
    B<int> b1;
    B<T> b2;
  };

  template<>
  struct C<int *>
  {
    B<int> b1;
  };

  template struct C<void>;
  template struct C<char *>;
}

namespace member_with_constrained_param_pack
{
  template<typename> concept X = true;

  template<typename ... T>
  struct C
  {
    template<X> struct B {};
    B<int> b1;
    B<T ...> b2;
  };

  template<typename ... T>
  struct C<T * ...>
  {
    template<X> struct B {};
    B<int> b1;
    B<T ...> b2;
  };

  template<>
  struct C<int *>
  {
    template<X> struct B {};
    B<int> b1;
  };

  template struct C<void>;
  template struct C<char *>;
}
