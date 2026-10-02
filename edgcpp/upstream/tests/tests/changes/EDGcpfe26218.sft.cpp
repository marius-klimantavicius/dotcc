//type:fp
//options_all:--c23
//remark:[6.5] C++-generating back end: ellipsis-only functions in C mode
// 4/5/23   [EDGcpfe/26218]
//
// C++-generating back end: ellipsis-only functions in C mode
//
// In configurations in which ALLOW_ELLIPSIS_ONLY_PARAM_IN_GENERATED_C is set
// to FALSE when generating code for a C translation unit, the C++-generating
// back end previously omitted the ellipsis in the declaration of a function
// whose parameter list contains only an ellipsis.  Although needed for the
// C-generating back end, this transformation was incorrect for the
// C++-generating back end, where the output is intended to match the input as
// nearly as possible.  This is now fixed.
void f(...);   // Now correctly output with the ellipsis
