//type:fp
//options_all:--c++14 -tused -A
   struct Aggr { int i; int j; };
    template<class T, int N> void n(T const(&)[N], T);

int main()
{
 n({{1},{2},{3}},Aggr()); // OK, T is Aggr, N is 3
}
