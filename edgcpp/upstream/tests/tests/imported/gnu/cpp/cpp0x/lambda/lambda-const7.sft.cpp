//type: fp
//options: --c++11 -w
// { dg-do compile { target c++11 } }
// { dg-options -w }

int main()
{
  const int i = 4;
  [] { constexpr int x = i; };
  [=] { &i; constexpr int x = i; };
  [&] { &i; constexpr int x = i; };
  [i] { &i; constexpr int x = i; };
  [&i] { &i; constexpr int x = i; };
}
