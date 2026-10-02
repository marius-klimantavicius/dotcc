//type:fn
//options:--c++23:--c++23 -A:--c++11 --gn 160100;fp:--c++11 --clang_version 210100;fp

struct C
{
  int i1, i2;
};

template<typename T>
int f(T t, C c)
{
  auto [ ... b ] = t;
  return (b + ... );
}

template int f(C, C);
