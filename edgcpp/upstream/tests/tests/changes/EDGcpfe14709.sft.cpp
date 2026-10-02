//type:fp
//options_all:--microsoft
//remark:[4.9] Microsoft-mode abort in is_special_rvalue_ref_generic_parameter_at_pos
// 11/26/13 [EDGcpfe/14709]
//
// Microsoft-mode abort in is_special_rvalue_ref_generic_parameter_at_pos
//
// The changes for EDGcpfe/14129 introduced a regression that caused the front
// end to abort in is_special_rvalue_ref_generic_parameter_at_pos with certain
// non-top-level function declarators in Microsoft-mode templates (requires
// support for rvalue references; e.g., with microsoft_version >= 1600).
//
// This is now fixed.
template<typename T> void (*f()) (int);  // Return type uses a function
                                         // declarator.
void (*pf)(int) =  f<void>();            // Previously triggered an
                                         // internal error.
