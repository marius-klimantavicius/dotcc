//type:fp
//options_all:--c++20
//remark:Attributes on concepts and [[deprecated]]
// 2/5/26   [EDGcpfe/25818,EDGcpfe/28688]
//
// Attributes on concepts and [[deprecated]]
//
// The front end now accepts attributes on concepts.  Initially, the only
// attribute accepted for a concept is [[deprecated]] (or the variant with a
// message).  This was added to the language through the defect resolution of the
// C++ committee's Core issue 2428.
template<typename T> concept C [[deprecated]] = true;
static_assert(C<int>);
