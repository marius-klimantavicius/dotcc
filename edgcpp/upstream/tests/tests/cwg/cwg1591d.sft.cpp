//type:fp
//options_all:--c++14 -tused -A
  template<int M, int N> void m(int const(&)[M][N]);

int main()
{
  m({{1,2},{3,4}});        // M and N both deduced to 2
}
