//type:cp
//options::--g++
//options_all:-tused

struct Base2
{
  virtual void coupled();
};

template <class T>
struct Base : public Base2 {};

template <class T>
struct Derived : public Base<T>
{
  Derived(int);
  using Base2::coupled;
};

template <class T>
Derived<T>::Derived(int)
{
   coupled();
}

Derived<int> foo(0);

