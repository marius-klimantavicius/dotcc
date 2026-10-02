//type:fp
//options_all:--c++17 -tused -A
template<class ... T> struct C {
  void f(int n = 0, T...);
};
C<int> c;   // OK, instantiates declaration void C​::​f(int n = 0, int)

//cwg: 2442
//title: Incorrect requirement for default arguments
//meeting: Belfast 11/19
//edg_status: Passes
