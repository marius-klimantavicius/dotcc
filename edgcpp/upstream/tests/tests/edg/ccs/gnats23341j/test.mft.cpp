//type:cp
//options_all:--definition_list_file=def_list
//source_files:def_list

struct X {};
struct Y {};
template<typename T> struct TX;
template<typename T, int N> struct TY;

template<typename T>
struct Base {
  Base();
  X bmem;
};

template<typename T, int N>
struct Derived : Base<X> {
  Derived();
  X dmem;
};

template<typename T>
Base<T>::Base()
{
  Derived<X, 0>();
}

template<typename T, int N>
Derived<T, N>::Derived()
  : dmem(TY<T, 0>().func2())
{
}

template<typename T> struct TX {
  virtual void func();
};

template<typename T, int N>
struct TY {
  X func2();
  template<typename U> X func3(U) throw(TX<U>);
};

template<typename T>
void TX<T>::func() {
  Base<X>();
}

template<typename T, int N>
X TY<T, N>::func2() {
  return TY<Y, N>().func3(0);
}

Derived<Y, 0> var;
