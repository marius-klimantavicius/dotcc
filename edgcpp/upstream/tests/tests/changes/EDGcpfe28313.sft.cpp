//type:fp
//options_all:--c++20
//remark:[6.8] Abort on type constraint with dependent non-type template parameter
// 10/14/25 [EDGcpfe/28313]
//
// Abort on type constraint with dependent non-type template parameter
//
// Previously, the front end would abort due to a failed assertion in
// update_template_param_symbol for a type constraint declared with a dependent
// non-type template parameter.
template<typename, int, auto>
concept C = true;
template<C<1, 2>>  // Previously triggered an assertion failure.  Now okay.
struct A;
