//type:fp
//options_all:--gnu=40500
//remark:[4.10] GNU compatibility: typedef redeclaration in system header files
// 4/29/14  [EDGcpfe/12740,EDGcpfe/14294]
//
// GNU compatibility: typedef redeclaration in system header files
//
// Versions of GNU earlier than 4.6.0 appear to allow a typedef redeclaration
// when both typedefs refer to pointer to function types and there is a
// discrepancy in the parameter types of the underlying function types.
// Such discrepancies are only ignored in system header files.  A change
// has been made to downgrade the error to a warning (which is typically
// suppressed because warnings are typically suppressed when parsing system
// headers).
// --gnu_version=40500:
# 2 "/usr/include/bits/types.h" 1 3 4
typedef void (*my_sig_t)();
typedef void (*__sighandler_t) (int);
typedef __sighandler_t my_sig_t;
