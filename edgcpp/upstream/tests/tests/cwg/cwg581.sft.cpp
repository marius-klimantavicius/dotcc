//type: fp
//options:
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
  class C {
    template <typename T>
    C(const T &) {}
  };
  template C::C<double>(const double &);

//cwg: 581
//title: Can a templated constructor be explicitly instantiated or specialized?
//meeting: Kona 02/19
//edg_status: Passes
