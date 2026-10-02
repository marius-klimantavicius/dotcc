//type:fp
//options_all:--c++20
//remark:[6.8] Spurious subsumption of disjunctive clauses in constraints
// 9/18/25  [EDGcpfe/28424]
//
// Spurious subsumption of disjunctive clauses in constraints
//
// This example previously reported an ambiguity because a bug in the subsumption
// algorithm caused the front end to mistakenly conclude that C3 subsumes C1.
// That is now fixed.
template<typename T> constexpr bool true_1 = true;
template<typename T> constexpr bool true_2 = true;
template<typename T> concept C1 = true_1<T>;
template<typename T> concept C2 = true_2<T>;
template<typename T> concept C3 = ((C1<T> || C2<T>) && C1<T*>) || C1<T>;
int g(C3 auto) { return 1; }
int g(C1 auto) { return 3; }
int r = g(0);  // Previously ambiguous.  Now okay.
