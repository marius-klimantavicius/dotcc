//type:fp
//options:--c++20 -tused -A
template<int i> struct C { /* ... */ };
C<{ 42 }> c1; // OK

//cwg: 2450
//title: braced-init-list as a template-argument
//meeting: Kona 11/23
//edg_status: EDGcpfe/26815
//fixed_in: 6.10
