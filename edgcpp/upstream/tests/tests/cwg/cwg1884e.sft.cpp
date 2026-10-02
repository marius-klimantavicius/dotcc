//type:fn
//options_all:--c++20 -tused -A
void g();            // #1
void g(int);         // OK: different entity from #1
int g();             // error: same entity as #1 with different type
