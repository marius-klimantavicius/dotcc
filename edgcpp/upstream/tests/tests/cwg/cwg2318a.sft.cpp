//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap

struct Aggr { int i; int j; };
template<class T, int N> void n(T const(&)[N], T);
  n({{1},{2},{3}},Aggr()); // OK, T is Aggr, N is 3

//cwg: 2318
//title: Nondeduced contexts in deduction from a braced-init-list
//meeting: Kona 02/19
//edg_status: EDGcpfe/20919
