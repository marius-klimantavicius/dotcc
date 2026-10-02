//type:fn
//options_all:--c++17 -A

struct C {};
C *p;
int main()
{
p->~decltype(*p); // error, expected class name before decltype
}

//cwg: 1753
//title: decltype-specifier in nested-name-specifier of destructor
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
