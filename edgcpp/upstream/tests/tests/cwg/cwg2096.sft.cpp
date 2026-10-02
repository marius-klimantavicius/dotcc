//type:fp
//options_all:--c++17 -tused -A
union A 
{ int x; 
  volatile int y;
};
int main()
{
     static_assert(__is_literal_type(A));
}

//cwg: 2096
//title: Constraints on literal unions
//meeting: Jacksonville 2/16
//edg_status: EDGcpfe/20908
//fixed_in: 6.1
