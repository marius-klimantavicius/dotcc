//type:fp
//options_all:--c++11
//remark:[4.9] C++11: Destructors for return values and decltype
// 1/7/14   [EDGcpfe/14756]
//
// C++11: Destructors for return values and decltype
//
// When a function call is the immediate operand of a decltype construct, the
// call does not produce a temporary, and therefore an inaccessible or deleted
// destructor associated with the return type does not make the call invalid.
// This rule is now implemented.
//
// This change to the C++ language was introduced by the committee's paper N3276.
// (See also the entry for EDGcpfe/11740 and EDGcpfe/14277 of 7/19/13.)
struct R { ~R() = delete; };
R g();
typedef decltype(g()) TR;  // Previously an error because the destructor
                           // is deleted.  Now accepted in C++11 modes.
