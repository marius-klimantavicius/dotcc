//type:fp
//options_all:--c++20
//remark:[6.7] Abort on non-dependent specialization of constrained member template
// 7/24/24  [EDGcpfe/26322,EDGcpfe/26494,EDGcpfe/27098]
//
// Abort on non-dependent specialization of constrained member template
//
// Previously, when a template definition referred to a non-dependent
// specialization of a constrained member template, the front end aborted with a
// failed assertion in get_substitution_pairs_for_template_class (or, with the
// changes for EDGcpfe/27472, in get_all_class_subst_pairs).
// --c++20:
template<typename> concept X = true;
template<typename>
struct C {
  template<X> struct B {};
  B<int> b;  // Previously triggered an internal error.  Now okay.
};
