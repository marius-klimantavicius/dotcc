//type:fn
//options_all:--microsoft --no_ms_permissive
using BOOL = int;
 
namespace N
{
       extern "C" void f(int* p, bool flag);
}
 
void g()
{
       N::f(nullptr, false);
}
 
extern "C" void f(int* p, BOOL flag)
{
       if (flag) *p = 13;
}

