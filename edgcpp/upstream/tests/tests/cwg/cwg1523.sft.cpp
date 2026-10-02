//type:fp
//options_all:--c++17 -tused -A
int main()
{ for (int i : {i}) ;
}

//cwg: 1523
//title: Point of declaration in range-based for
//meeting: Toronto 7/17
//edg_status: EDGcpfe/22208
