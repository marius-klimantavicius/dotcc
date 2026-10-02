//type:fp
//options_all:--c++20
//remark:[5.1] C++20: const mismatch with defaulted copy constructor (core issue 1331)
// 12/6/18  [EDGcpfe/20010]
//
// C++20: const mismatch with defaulted copy constructor (core issue 1331)
//
// In C++17 and earlier, if the declared type of an explicitly-defaulted
// function is not the same as if it had been implicitly declared, then it
// is ill-formed.  In C++20, such functions are defined as deleted.
// Exceptions are made for two cases where it's desirable for a defaulted
// definition to remain ill-formed: an assignment operator with a
// mismatched return type, and an assignment operator with a parameter
// type that's not a reference.
//
// A's copy constructor was ill-formed before C++20.  In C++20 it is defined
// as deleted. This has now been implemented.
struct A{
    A(volatile A&) = default;
};
