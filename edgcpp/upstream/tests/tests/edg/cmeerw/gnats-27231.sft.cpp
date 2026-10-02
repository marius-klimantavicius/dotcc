//type:fp
//options:--c++11 --no_il_lower --il_display
//filter:awk '/^file-scope (param-type)@/{f=1}/^$/{if (f) print $0; f=0}f' | grep -E -e '^(param_num|is_parameter_pack|is_pack_element|name|type):' -e '^file-scope ' -e '^$' | sed -e 's/@[0-9a-f]*//'

struct C {
  int f(int);
};
template<typename ... Ts>
struct D {
  template<typename T, int (T::*)(Ts ... args)>
  static int f(Ts ... ts);
};
int i = D<int>::f<C, &C::f>(1);
