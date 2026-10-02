//remark:Nested generic lambdas
//options:--c++17 -tused;fp

struct A
{
 static constexpr int b = 0;
};
template<typename F>
void func(F &&f)
{
 f(A{});
}
int foo0()
{
 int c = 0;
 func
   ([&] (auto id1)
    {
      func
        ([&] (auto id2)
         {
             [&] { c = 1; } ();
         });
    });
 return c;
}
