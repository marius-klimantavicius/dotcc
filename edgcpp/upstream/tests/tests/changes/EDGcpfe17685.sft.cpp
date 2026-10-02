//type:fn
//options_all:--no_exc_spec_in_func_type
//remark:[4.14] C++17: Exception specifications as part of function types
// 8/3/17   [EDGcpfe/17685,EDGcpfe/18637]
//
// C++17: Exception specifications as part of function types
//
// C++17 makes exception specifications part of the function type.  The front end
// now implements that behavior in C++17 mode or when the new command-line option
// --exc_spec_in_func_type is specified (the C++17 behavior can also be overridden
// with --no_exc_spec_in_func_type).
//
// Here, the function type deduced for T is different for both parameters because
// exception specifications are now part of the type.
//
// C++17 also removes dynamic exception specifications.  In modes that make
// exception specifications part of the type, such dynamic exception
// specifications now elicit a discretionary error (and if the effective
// severity of the message is lower than an error, the exception specification
// is ignored).
//
// Making exception specifications part of function types affects the ABI.
//
// has to reflect the "noexcept".  The configuration macro
// EXC_SPEC_IN_FUNC_TYPE_ENABLING_POSSIBLE can be set to FALSE to disable this
// feature in all modes, thereby maintaining ABI stability at the expense of
// standards conformance.
//
// To support the throwing of pointers to function types with noexcept
// exception specifications in configurations where lowering generates
// exception tables, a new bit, ETS_IS_POINTER_TO_NOEXCEPT_FUNCTION, is introduced
// to indicate the presence of a noexcept exception specification on the
// underlying function type.  Due to the lack of bits in the run time library's
// an_ETS_flag_set type, ETS_IS_POINTER_TO_NOEXCEPT_FUNCTION uses the same bit
// as ETS_IS_ELLIPSIS with (ETS_IS_POINTER_TO_MEMBER_FUNCTION | ETS_IS_POINTER)
// being used to distinguish between the two cases.  This is a subtle IL CHANGE.
void g1() noexcept;
void g2();
template<typename T> int f(T*, T*);
int x = f(g1, g2);    // Okay in C++14.  Now an error in C++17 mode.
