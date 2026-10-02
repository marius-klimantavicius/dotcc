//type:fp
//options:--c++20 -tused -A

template <int* x = {}> struct X {};

//cwg: 2049
//title: List initializer in non-type template default argument
//meeting: Kona 11/23
//edg_status: EDGcpfe/26815
//fixed_in: 6.10
