//remark:Abbreviated function templates
//options:--c++20;fp

auto f(auto i)
{
   return 2 * i;
}

void d()
{
   f(1);
   f(1.0f);
}
