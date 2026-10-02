//type:cp
//options:--c++14:--gnu_version 80000

struct Inner {};

struct Outer
{
  Inner& GetInner();
};

struct Sample
{
  Sample(Outer& outer) : inner{outer.GetInner()} {}
  Inner& inner;
};

Outer outer;
Sample s(outer);
