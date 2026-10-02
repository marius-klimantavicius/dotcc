//type:cp
//options:--c++11

auto (*fn_p)() -> void;

void f()
{
  fn_p = nullptr;
}
