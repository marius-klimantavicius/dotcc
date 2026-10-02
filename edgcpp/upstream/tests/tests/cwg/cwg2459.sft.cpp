//type:fp
//options:--c++20 -tused -A

template<int> struct X {};
X<{}> x;

//cwg: 2459
//title: Template parameter initialization
//meeting: Kona 11/23
//edg_status: EDGcpfe/26815
//fixed_in: 6.10
