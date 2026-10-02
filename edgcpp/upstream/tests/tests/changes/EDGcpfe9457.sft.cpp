//type:fp
//options_all:--g++
//remark:[4.1] GNU C++ compatibility: Visibility of a static const member in its initializer
// 1/19/09  [EDGcpfe/9457]
//
// GNU C++ compatibility: Visibility of a static const member in its initializer
//
// In GNU C++ mode, a static const class member with an in-class initializer is
// now invisible until the end of the initializer.  (This same behavior was
// already implemented in Microsoft bugs mode: See the Changes entry of 11/7/07.)
//
// Note that this matches the GNU behavior, but GCC often diagnoses such cases
// because it checks that certain identifiers used in a class definition retain
// their meaning when reconsidered in the context of the completed definition.
// (This check is permitted by the C++ standard but not currently performed by
// the EDG front end.)  E.g., the example above elicits an error from g++, but
// the following similar example does not:
enum { x = 3 };
struct y { static int const x = x; };  // Now accepted in GNU mode.
