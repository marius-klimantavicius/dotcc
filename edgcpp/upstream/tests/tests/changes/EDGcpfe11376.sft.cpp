//type:fp
//remark:[4.3] Abort on __asm function body starting with a comment
// 2/25/11  [EDGcpfe/11376]
//
// Abort on __asm function body starting with a comment
//
// When ASM_FUNCTION_ALLOWED and INCLUDE_COMMENTS_IN_ASM_FUNC_BODY are TRUE, the
// front end executed an invalid memcpy operation (resulting in an abort) when
// processing an __asm function body starting with a comment.
//
// This is now fixed.
__asm int f() { /**/ }
  // Previously triggered an abort in some configurations.
