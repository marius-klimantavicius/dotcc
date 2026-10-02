//type:fp
//options_all:--c++20
//remark:[6.8] C++-generating back end: variable template with default lambda argument
// 2/12/25  [EDGcpfe/27918]
//
// C++-generating back end: variable template with default lambda argument
//
// In cases when a variable template is declared with a non-type template
// parameter whose default argument is a lambda expression and an instance of
// that template uses the default argument, the C++-generating back end could
// abort with a segfault or put out the defaulted argument as an explicit
// expression using an undeclared temporary name of the form __T12345678,
// depending on the configuration.  This is now fixed.
// --c++20:
template<class T, auto = []{}> int v = 1;
int n1 = v<int>;   // Previously segfaulted or generated the second argument
                   // with an undeclared temporary name, now preserves the
                   // original source form
