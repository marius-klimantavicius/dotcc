//type:fp
//options_all:-tused --c++11

template<int X>
int recurse() {
  return (0 << X * 1000) + recurse<X + 1>();
}

template<>
int recurse<5>() {
  return 0;
}

int x = recurse<1>();
