//type:fp
//options_all:--c++
//remark:[6.6] Partially-specialized non-type template argument involving a template parameter
// 8/17/23  [EDGcpfe/19237,EDGcpfe/25886,EDGcpfe/26582]
//
// Partially-specialized non-type template argument involving a template parameter
//
// The resolution of Core issue 1315 (treated as a defect report against previous
// C++ standards) removed a restriction on a partially-specialized non-type
// argument expression to not involve a template parameter of the partial
// specialization.
template<int I, int J> struct A;
template<int I>
struct A<I, I*2> { };  // Previously a spurious error.  Now okay.
