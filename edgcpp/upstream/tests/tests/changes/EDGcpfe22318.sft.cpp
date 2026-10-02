//type:fn
//options_all:--c++14
//remark:[6.8] Abort on diagnostic for partially-specialized variable template
// 4/17/25  [EDGcpfe/22318,EDGcpfe/24369,EDGcpfe/27900]
//
// Abort on diagnostic for partially-specialized variable template
//
// Previously, diagnostics referring to partially-specialized variable templates
// used the template arguments for the primary template but the template parameter
// names from the partial specialization declaration.  In cases where the partial
// specialization has more template parameters than the primary template, this
// would trigger an internal error in form_template_arg_info.  Now, variable
// template specializations are output as template-ids in diagnostics (such as
// "u<int [1]>" and "u<int [1][2]>" in the example above).
template<int I, int J> struct C { };
template<typename T>   bool u;
template<int I>        bool u<int[I]>;
template<int I, int J> bool u<int[I][J]>;
constexpr bool v1 = u<int[1]>;     // Previously output as
                                   // "u [with I=int [1]]" in the diagnostic.
constexpr bool v2 = u<int[1][2]>;  // Previously triggered an internal error.
