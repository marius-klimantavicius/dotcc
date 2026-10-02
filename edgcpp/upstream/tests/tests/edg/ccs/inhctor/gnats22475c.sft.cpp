//type:cp
//options:--c++11:--c++17:--gnu_version 90300:--clang_version 80000

template<typename T> struct Base { Base(T val) {} };

template<typename T> struct Derived : public Base<T> {
  using Derived::NotABase::NotABase;
};
