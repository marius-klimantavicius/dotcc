//type:fp
//options_all:--c++11
//remark:[4.8] Prefix attributes on exception handler parameters
// 6/26/13  [EDGcpfe/13502]
//
// Prefix attributes on exception handler parameters
//
// The front end now accepts prefix attributes on exception handler parameters.
void f() try {
} catch ([[]] int) {  // Now accepted in C++11 mode.
}
