//type:fp
//options:--c++11:--c++20:--c++20 --gn 150100:--c++20 --clang_version 210100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  template<typename> struct B;
  template<typename T> using A = B<T>;
  template<typename T> struct C {
    using AT = A<T>;
    void f(C<AT> *);
  };
  template<typename T>
  void C<T>::f(C<B<T>> *) {}
}

namespace alias_template
{
  template<typename> struct B;
  template<typename T> using A = B<T>;
  template<typename T> struct C
  {
    using AT = A<T>;

    void f(C<AT> &);
    void g(C<AT> &);
    void h(C<A<T>> &);
    void i(C<B<T>> &);
    void j(C<A<T>> &);
    void k(C<B<T>> &);
  };

  template<typename T>
  void C<T>::f(C<A<T>> &)
  { }

  template<typename T>
  void C<T>::g(C<B<T>> &)
  { }

  template<typename T>
  void C<T>::h(C<B<T>> &)
  { }

  template<typename T>
  void C<T>::i(C<A<T>> &)
  { }

  template<typename T>
  using D = B<T>;

  template<typename T>
  void C<T>::j(C<D<T>> &)
  { }

  template<typename T>
  void C<T>::k(C<D<T>> &)
  { }
}

namespace const_alias_template
{
  template<typename> struct B;
  template<typename T> using A = const B<T>;
  template<typename T> struct C
  {
    using AT = A<T>;

    void f(C<AT> &);
    void g(C<AT> &);
    void h(C<A<T>> &);
    void i(C<const B<T>> &);
    void j(C<A<T>> &);
    void k(C<const B<T>> &);
  };

  template<typename T>
  void C<T>::f(C<A<T>> &)
  { }

  template<typename T>
  void C<T>::g(C<const B<T>> &)
  { }

  template<typename T>
  void C<T>::h(C<const B<T>> &)
  { }

  template<typename T>
  void C<T>::i(C<A<T>> &)
  { }

  template<typename T>
  using D = B<T>;

  template<typename T>
  void C<T>::j(C<const D<T>> &)
  { }

  template<typename T>
  void C<T>::k(C<const D<T>> &)
  { }
}

namespace no_template_args
{
  template<typename> struct B;
  template<typename T> using A = B<T>;
  template<typename T> struct C
  {
    using AT = A<T>;

    void f(AT &);
    void g(AT &);
    void h(A<T> &);
    void i(B<T> &);
    void j(A<T> &);
    void k(B<T> &);
  };

  template<typename T>
  void C<T>::f(A<T> &)
  { }

  template<typename T>
  void C<T>::g(B<T> &)
  { }

  template<typename T>
  void C<T>::h(B<T> &)
  { }

  template<typename T>
  void C<T>::i(A<T> &)
  { }

  template<typename T>
  using D = B<T>;

  template<typename T>
  void C<T>::j(D<T> &)
  { }

  template<typename T>
  void C<T>::k(D<T> &)
  { }
}

namespace class_template
{
  template<typename> struct B;
  template<typename T> using A = B<T>;
  template<typename T> struct C
  {
    using BT = B<T>;

    void f(C<BT> &);
    void g(C<BT> &);
  };

  template<typename T>
  void C<T>::f(C<A<T>> &)
  { }

  template<typename T>
  void C<T>::g(C<B<T>> &)
  { }
}
