//type:fn
//options:--c++20:--g++
//options_all:-tused

template<class T> void f()
{
  T v[1][1] = {{ [0][0] = 0 }};
};

void g() {
  f<short>();
}
