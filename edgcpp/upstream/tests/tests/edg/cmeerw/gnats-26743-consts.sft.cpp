//type:fp
//options:--c++20

struct A
{
  const int &r;
  const int *p;
};

template<const int *>
struct CP
{ };

template<const int &>
struct CR
{ };

template<A>
struct CA
{ };


struct B
{
  int i;
  int j;
} b;

struct X
{
  int x;
};

struct D : X, B
{ } d;

struct E
{
  D arr[3];
} e;

int i;

CR<i> cr_i;
CP<&i> cp_i;

CR<b.i> cr_b_i;
CP<&b.i> cp_b_i;
CR<b.j> cr_b_j;
CP<&b.j> cp_b_j;

CR<d.i> cr_d_i;
CP<&d.i> cp_d_i;
CR<d.j> cr_d_j;
CP<&d.j> cp_d_j;

CR<e.arr[0].i> cr_e_0_i;
CP<&e.arr[0].i> cp_e_0_i;
CR<e.arr[0].j> cr_e_0_j;
CP<&e.arr[0].j> cp_e_0_j;

CR<e.arr[1].i> cr_e_1_i;
CP<&e.arr[1].i> cp_e_1_i;
CR<e.arr[1].j> cr_e_1_j;
CP<&e.arr[1].j> cp_e_1_j;

CA<A{ i, &i }> ca_i_i;
CA<A{ b.i, &b.i }> ca_b_i_b_i;
CA<A{ b.j, &b.j }> ca_b_j_b_j;
CA<A{ d.i, &d.i }> ca_d_i_d_i;
CA<A{ d.j, &d.j }> ca_d_j_d_j;

CA<A{ e.arr[0].i, &e.arr[0].i }> ca_e_0_i_e_0_i;
CA<A{ e.arr[0].j, &e.arr[0].j }> ca_e_0_j_e_0_j;
CA<A{ e.arr[1].i, &e.arr[1].i }> ca_e_1_i_e_1_i;
CA<A{ e.arr[1].j, &e.arr[1].j }> ca_e_1_j_e_1_j;
