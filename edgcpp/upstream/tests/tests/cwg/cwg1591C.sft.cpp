//type:fn
//options_all:--c++14 -tused -A
  struct Aggr { int i; int j; };
  template<int N> void k(Aggr const(&)[N]);

int main()
{
       k({1,2,3});              // error: deduction fails, no conversion from int to Aggr
  k({{1},{2},{3}});        // OK, N deduced to 3
}
