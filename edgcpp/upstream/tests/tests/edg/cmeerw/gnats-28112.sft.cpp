//type:fp
//options:--c++14:--c++20:--c++20 --gn 150100:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  template<typename T>
  struct C {
    T t;
  };
  template<typename T> C<T> var;
  template<> C<void *> var<void>;
}

namespace explicit_specializations
{
  template<typename T>
  struct C
  {
    T t;
  };

  template<typename T> C<T> var{};

  template<> void *var<void>{};
}

namespace explicit_specializations_inst
{
  template<typename T>
  struct C
  {
    T t;
  };

  template<typename T> C<T> var{ };

  template<> C<void *> var<void>{ };

  auto v = var<void>;
}

namespace explicit_specializations_inst_array
{
  template<typename T>
  struct C
  {
    T t;
  };

  template<typename T> C<T> var{};

  template<> C<void *> var<void>[2] = { };

  auto v = var<void>;
}

#ifdef __cpp_deduction_guides
namespace deduced_class_type
{
  template<typename T>
  struct C
  {
    C(T);

    T t;
  };

  template<typename T> C var{1};

  template<> void *var<void>{};
}
#endif

namespace not_used
{
  template<typename T>
  struct C
  {
    T t;
  };

  template<typename T> C<T> var{};
  decltype(var<void>) *p = 0;   // GCC actually considers this a use
}

namespace explicit_instantiation_declaration
{
  template<typename T>
  struct C
  {
    T t;
  };

  template<typename T> C<T> var{};
  extern template C<void> var<void>; // GCC also considers this a use
}
