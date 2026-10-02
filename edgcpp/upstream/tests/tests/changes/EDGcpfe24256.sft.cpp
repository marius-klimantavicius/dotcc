//type:fp
//options_all:--c++20
//remark:[6.6] Partial specialization with constrained placeholder non-type template parameter
// 11/15/23 [EDGcpfe/24256,EDGcpfe/26665]
//
// Partial specialization with constrained placeholder non-type template parameter
//
// Previously, the front end did not consider a partial specialization with a
// non-type template parameter of constrained placeholder type to be more
// specialized than the primary template.
template<class T> concept C = true;
template<auto> struct B;
template<C auto V>
struct B<V> { };  // Previously a spurious error.  Now okay.
