//type:fn
//options_all:--c11
//remark:[6.0] C11-mode abort on _Generic construct that can match multiple types
// 10/23/19 [EDGcpfe/16509,EDGcpfe/21952]
//
// C11-mode abort on _Generic construct that can match multiple types
//
// The front end previously aborted with an internal error on certain C11 _Generic
// constructs that can match multiple cases.
//
// That is now fixed.
int r = _Generic((void (*)())0,
                 void (*)(int)  : 0,
                 void (*)(void) : 0);  // Previously aborted.  Now an
                                       // ordinary error.
