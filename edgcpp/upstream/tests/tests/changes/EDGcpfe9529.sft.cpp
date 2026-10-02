//type:fp
//remark:[4.1] Abort on Microsoft-mode in-class specialization appearing in a class template
// 2/12/09  [EDGcpfe/9529]
//
// Abort on Microsoft-mode in-class specialization appearing in a class template
//
// When performing nonclass prototype instantiations in Microsoft mode (i.e.,
// with the command-line options "--microsoft --parse_templates") in
// configurations that have TEMPLATE_INSTANTIATIONS_IN_SOURCE_SEQUENCE_LISTS
// and FRIEND_AND_MEMBER_DEFINITIONS_MAY_BE_MOVED_OUT_OF_CLASS set to TRUE,
// the front end aborted in inline_function_fixup_for_class when dealing with
// a Microsoft-mode in-class specialization appearing in a class template.
//
// This is now fixed.
template<class T> struct S {
  template<class U> T f(U) {}
  template<> T f(int) {}  // Triggered an abort under some circumstances.
};
