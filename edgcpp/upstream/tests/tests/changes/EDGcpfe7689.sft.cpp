//type:fp
//options_all:--c++11
//remark:[4.7] C++11: Delegating constructors
// 4/23/13  [EDGcpfe/7689,EDGcpfe/12842]
//
// C++11: Delegating constructors
//
// In C++11 mode, the front end now accepts "delegating constructors".
// See paper N1986.
//
// This feature can also be enabled in non-C++11 modes with the new command-line
// option --delegating_constructors.
//
// Note that in the IA-64 ABI configuration, when using lowering, delegating
// constructors that have virtual base classes are implemented by invoking
// a new cdk_delegation constructor (which is specific to the EDG front end
// -- i.e., it's not part of the IA-64 ABI).  This constructor is invoked
// by both the complete and subobject constructors and contains special
// treatment to ensure that the proper (i.e., complete or subobject) target
// constructor is called.  In certain cases where the delegating constructor
// body is empty, use of this constructor is optimized away.
//
// A new "delegation" destructor is used in both IA-64 ABI and Cfront
// configurations to determine at run-time whether the complete object or
// the subobject is being destroyed and invoke the appropriate destructor
// (or in the Cfront case, invoke the destructor with the appropriate second
// argument).
//
// In the IA-64 ABI, these new cdk_delegation constructor and destructor routines
// are mangled with "C9" and "D9" respectively.  The delegation destructor is
// unnamed in the Cfront ABI (a temporary name is used).
//
// The use of a delegation destructor relies on a change in the
// run-time library, and thus is enabled only when ABI_COMPATIBILITY_VERSION is
// 407 or greater (in configurations where lowering does exception handling).
struct S {
  S(int);
  S(): S(0) {}  // Default constructor for S delegates to constructor
};              // S::S(int).
