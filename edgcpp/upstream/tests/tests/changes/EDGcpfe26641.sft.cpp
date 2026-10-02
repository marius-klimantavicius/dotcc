//type:fp
//options_all:--c++20
//remark:[6.8] Variable template partial specializations using requires-clauses
// 7/30/25  [EDGcpfe/26641,EDGcpfe/27432]
//
// Variable template partial specializations using requires-clauses
//
// Previously, the front end mistakenly treated the last partial specialization
// declaration as a redeclaration of the partial specialization (1).  That
// resulted in a spurious error about the requires-clause being incompatible.
// Now it is correctly treated as a distinct partial specialization since the
// requires-clause is distinct.
template<typename> int v;
template<typename T> int v<T*>;                // (1)
template<typename T> requires true int v<T*>;  // Previously an error.
