//type:fp
//remark:[4.14] Abort in determine_function_viability on empty pack expansion
// 6/12/17  [EDGcpfe/18471]
//
// Abort in determine_function_viability on empty pack expansion
//
// In some overload resolution cases involving a variadic function template
// candidate with an empty pack expansion but too many arguments (a somewhat
// unusual combination), the front end aborted with an internal error in
// determine_function_viability (overload.c).
//
// This is now fixed.
template<class T> T* f(unsigned, long const &);
template<class T, class ... Ts> T* f(Ts ..., const long&);
char *b = f<char>(0, 'x');  // Previously triggered an abort.  Now okay.
