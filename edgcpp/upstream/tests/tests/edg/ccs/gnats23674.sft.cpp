//type:cp
//options::--gnu_version 50400
//options_all:--c++14

template <typename T, int size>
struct arr {  T arr[size]; };

struct Ele  {  int i; };

template <int... Is>
auto doit() {
  return arr<Ele, sizeof...(Is)>{{Ele{Is}...}};
}
auto returns = doit<1>();
