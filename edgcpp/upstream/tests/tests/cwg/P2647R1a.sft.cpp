//type:fp
//options_all:--c++23 -tused -A
constexpr char test()
{
   static const int x = 5;
   static constexpr char c[] = "Hello World";
   return *(c+x);
}
static_assert(' ' == test());
