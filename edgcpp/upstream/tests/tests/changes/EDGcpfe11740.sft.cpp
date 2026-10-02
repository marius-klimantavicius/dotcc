//type:fp
//options_all:--c++11
//remark:[4.8] C++11: Incomplete return types and decltype
// 7/19/13  [EDGcpfe/11740,EDGcpfe/14277]
//
// C++11: Incomplete return types and decltype
//
// Toward the end of the C++11 standardization process, the committee adopted a
// rule that the return type in a function call need not be complete if the
// function call appears as the immediate operand of a decltype construct (see
// the committee's paper N3276).  This behavior is now implemented.
struct S;
S f();
decltype(f()) *p;  // Previously an error, now okay.
