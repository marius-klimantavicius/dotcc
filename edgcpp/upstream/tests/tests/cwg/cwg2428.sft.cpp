//type:fp
//options_all:--c++20 -tused -A
template <class T> concept C [[deprecated]] = 29 + 37 == 66;

int x[C<short>];

//cwg: 2428
//title: Deprecating a concept
//meeting: Kona 11/22
//edg_status: EDGcpfe/25818
//fixed_in: 6.9
