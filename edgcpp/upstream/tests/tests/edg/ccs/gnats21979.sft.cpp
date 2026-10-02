//type:cp
//options:--c++11

struct S
{
  S(int);
  S(const S& param) = delete;
  S(S&& param);
};

void func(void)
{
  1 ? (1,S(1)) : (1,S(2));
  1 ? (1,S(1)) : (S(2));
  1 ? (S(1)) : (1,S(2));
  1 ? (S(1),1) : (1,S(2));
}
