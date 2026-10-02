//type:fp
//options:--c++03 --gn 150100:--c++03 --clang_version 190100

template<typename ... Ts>
int f(Ts ... vs)
{
  Ts ... [0] s;
  return vs ... [0];
}

template int f(int);
