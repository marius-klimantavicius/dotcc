//type:fp
//options:--c++03 --strict:--c++11 --strict:--c++14 --strict:--c++17 --strict:--c++20 --strict:--c++23 --strict
namespace impl_static
{
  namespace non_tmpl
  {
    struct C
    {
      void *operator new(__SIZE_TYPE__);
      void operator delete(void *);
    };

    void *(*pnew)(__SIZE_TYPE__) = &C::operator new;
    void (*pdelete)(void *) = &C::operator delete;
  }

  namespace tmpl
  {
    template<typename T>
    struct C
    {
      void *operator new(__SIZE_TYPE__);
      void operator delete(void *);
    };

    void *(*pnew)(__SIZE_TYPE__) = &C<int>::operator new;
    void (*pdelete)(void *) = &C<int>::operator delete;
  }

  namespace non_tmpl_tmpl_oper
  {
    struct C
    {
      template<typename U>
      void *operator new(__SIZE_TYPE__, U);

      template<typename U>
      void operator delete(void *, U);
    };

    void *(*pnew)(__SIZE_TYPE__, int) = &C::operator new<int>;
    void (*pdelete)(void *, int) = &C::operator delete<int>;
  }

  namespace tmpl_spec_tmpl_oper
  {
    template<typename T>
    struct C
    {
      template<typename U>
      void *operator new(__SIZE_TYPE__, U);

      template<typename U>
      void operator delete(void *, U);
    };

    void *(*pnew)(__SIZE_TYPE__, int) = &C<int>::operator new<int>;
    void (*pdelete)(void *, int) = &C<int>::operator delete<int>;
  }

  namespace non_tmpl_spec_oper
  {
    struct C
    {
      template<typename U>
      void *operator new(__SIZE_TYPE__, U);

      template<>
      void *operator new(__SIZE_TYPE__, int);

      template<typename U>
      void operator delete(void *, U);

      template<>
      void operator delete(void *, int);
    };

    void *(*pnew)(__SIZE_TYPE__, int) = &C::operator new<int>;
    void (*pdelete)(void *, int) = &C::operator delete<int>;
  }

  namespace tmpl_spec_spec_oper
  {
    template<typename T>
    struct C
    {
      template<typename U>
      void *operator new(__SIZE_TYPE__, U);

      template<>
      void *operator new(__SIZE_TYPE__, int);

      template<typename U>
      void operator delete(void *, U);

      template<>
      void operator delete(void *, int);
    };

    void *(*pnew)(__SIZE_TYPE__, int) = &C<int>::operator new<int>;
    void (*pdelete)(void *, int) = &C<int>::operator delete<int>;
  }
}

namespace expl_static
{
  namespace non_tmpl
  {
    struct C
    {
      static void *operator new(__SIZE_TYPE__);
      static void operator delete(void *);
    };

    void *(*pnew)(__SIZE_TYPE__) = &C::operator new;
    void (*pdelete)(void *) = &C::operator delete;
  }

  namespace tmpl
  {
    template<typename T>
    struct C
    {
      static void *operator new(__SIZE_TYPE__);
      static void operator delete(void *);
    };

    void *(*pnew)(__SIZE_TYPE__) = &C<int>::operator new;
    void (*pdelete)(void *) = &C<int>::operator delete;
  }

  namespace non_tmpl_tmpl_oper
  {
    struct C
    {
      template<typename U>
      static void *operator new(__SIZE_TYPE__, U);

      template<typename U>
      static void operator delete(void *, U);
    };

    void *(*pnew)(__SIZE_TYPE__, int) = &C::operator new<int>;
    void (*pdelete)(void *, int) = &C::operator delete<int>;
  }

  namespace tmpl_spec_tmpl_oper
  {
    template<typename T>
    struct C
    {
      template<typename U>
      static void *operator new(__SIZE_TYPE__, U);

      template<typename U>
      static void operator delete(void *, U);
    };

    void *(*pnew)(__SIZE_TYPE__, int) = &C<int>::operator new<int>;
    void (*pdelete)(void *, int) = &C<int>::operator delete<int>;
  }

  namespace non_tmpl_spec_oper
  {
    struct C
    {
      template<typename U>
      static void *operator new(__SIZE_TYPE__, U);

      template<>
      void *operator new(__SIZE_TYPE__, int);

      template<typename U>
      static void operator delete(void *, U);

      template<>
      void operator delete(void *, int);
    };

    void *(*pnew)(__SIZE_TYPE__, int) = &C::operator new<int>;
    void (*pdelete)(void *, int) = &C::operator delete<int>;
  }

  namespace tmpl_spec_spec_oper
  {
    template<typename T>
    struct C
    {
      template<typename U>
      static void *operator new(__SIZE_TYPE__, U);

      template<>
      void *operator new(__SIZE_TYPE__, int);

      template<typename U>
      static void operator delete(void *, U);

      template<>
      void operator delete(void *, int);
    };

    void *(*pnew)(__SIZE_TYPE__, int) = &C<int>::operator new<int>;
    void (*pdelete)(void *, int) = &C<int>::operator delete<int>;
  }
}
