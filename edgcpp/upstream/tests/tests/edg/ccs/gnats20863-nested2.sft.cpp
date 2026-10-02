//type:rp
//options::--gnu_version 80000:--clang
//options_all:--c++11 --no_exceptions
struct bug { bug*a = [&]{ return [=]{return this;}(); }(); };
struct A {
  A* a = [&]{ return [=]{ return this; }(); }();
  bool identical = a == this;
};

int main()
{
  if (!A().identical)
    return 1;

  return 0;
}
