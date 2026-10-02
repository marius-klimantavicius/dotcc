//type:cp
//options:--c++14:--gnu_version 80000

struct Inner {};

struct Outer
{
  Inner& GetInner();
};

template<typename T>
struct Sample
{
  Sample(T& outer) : inner{outer.GetInner()} {}
  Inner& inner;
};

Outer outer;
Sample<Outer> s(outer);
