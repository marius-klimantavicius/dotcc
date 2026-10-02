//type:fp
//options:--c++20

namespace std {
  typedef unsigned long size_t;

  template<typename _E> struct initializer_list {
    typedef _E value_type;
    typedef _E const &reference;
    typedef _E const &const_reference;
    typedef _E const *iterator;
    typedef _E const *const_iterator;
    typedef std::size_t size_type;

    constexpr initializer_list(): __array(0), __length((size_t)0) {}

    constexpr size_type size() const { return this->__length; }
    constexpr iterator begin() const { return this->__array; }
    constexpr iterator end() const {
      return this->__length == 0 ? this->__array
             : this->__array + this->__length;
    }
  private:
    iterator __array;
    size_type __length;
    constexpr initializer_list(_E const __a[], std::size_t __l)
      : __array(__a), __length(__l) {}

  };

  template<typename _E> constexpr
  typename initializer_list<_E>::iterator begin(initializer_list<_E> il) {
    return il.begin();
  }

  template<typename _E> constexpr
  typename initializer_list<_E>::iterator end(initializer_list<_E> il) {
    return il.end();
  }
}

namespace local_classes_enums
{
  template <class T> T f(const std::initializer_list<T>& list, int off = 0)
  {
    return (T) f(list, off + 1);
  }

  void g()
  {
    {
      struct C { };
      enum E { };

      f<E>({});
      f<C>({});
    }

    {
      struct C { };
      enum E { };

      f<E>({});
      f<C>({});
    }
  }
}

namespace dpdt_decltype_in_non_type_tmpl_param
{
  int x;

  template<decltype([]{return int();}()) * ip>
  struct A { };

  template <class T> auto f1(T x) -> decltype(x == new A<&dpdt_decltype_in_non_type_tmpl_param::x>)
  { return true; }

  void g()
  {
    new A<&x>;
  }
}
