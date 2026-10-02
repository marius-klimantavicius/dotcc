//type:fp
//options_all:--c++14
//remark:[4.12] Exception specifications and defaulted special member functions
// 9/2/16   [EDGcpfe/16241,EDGcpfe/17453,EDGcpfe/17474]
//
// Exception specifications and defaulted special member functions
//
// Previously, declaring a defaulted special member function with an exception
// specification that does not match that of a corresponding generated special
// member always resulted in an error.  Now, in C++14 mode (or GNU C++11 mode),
// when the special member is defaulted in the class definition, the member
// becomes deleted.
//
// This change reflects the resolution of Core issue 1778.
struct S {
  ~S() throw(int) = default;  // Previously an error.  Now accepted in
};                            // C++14 mode.
