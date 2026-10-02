//type:fp
//options:--c++20 --gn 110400;fn:--c++20 --gn 120000:--c++20 --gn 120100

void f()
{
  auto v = __builtin_inff16();
}
