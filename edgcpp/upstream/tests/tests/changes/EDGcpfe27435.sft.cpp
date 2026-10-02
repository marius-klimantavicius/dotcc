//type:fp
//options_all:--c++20
//remark:[6.7] Pack expansions in nested template arguments of concept-ids
// 10/23/24 [EDGcpfe/27435]
//
// Pack expansions in nested template arguments of concept-ids
//
// Previously, the front end would issue a spurious error when a concept-id
// appearing in a default argument contains a pack expansion in a nested template
// argument.
template<typename> struct B { };
template<typename ...> concept X = true;
template<typename ... Ts,
         bool = X<B<Ts> ...>>  // Previously a spurious error.  Now okay.
void f();
