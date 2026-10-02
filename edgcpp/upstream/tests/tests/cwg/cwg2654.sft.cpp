//type:fp
//options_all:--c++23 -A -W
volatile int v = 1;
int main()
{
v +=5;
}

//cwg: 2654
//title: Un-deprecation of compound volatile assignments
//meeting: Kona 11/22
//edg_status: EDGcpfe/25847
//fixed_in: 6.10
