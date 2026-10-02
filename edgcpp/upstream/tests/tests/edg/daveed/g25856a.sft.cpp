//remark:non-auto vars and constant evaluation
//options:--c++20;fn:--c++23;fp

constexpr char test()
{
   static const int x = 5;
   static constexpr char c[] = "Hello World";
   return *(c+x);
}
static_assert(' ' == test());
