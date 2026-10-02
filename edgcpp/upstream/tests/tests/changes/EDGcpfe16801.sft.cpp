//type:fp
//remark:[4.12] Core issue 393: Reference/pointer to unknown-bound array in parameter type
// 5/19/16  [EDGcpfe/16801]
//
// Core issue 393: Reference/pointer to unknown-bound array in parameter type
//
// C++ originally prohibited parameters that are pointers or references to arrays
// of unknown bound (i.e., a parameter declaration like "int (&p)[]").  The
// resolution of Core issue 393 (and its duplicate 550) lifts that restriction
// and the front end now permits such parameters in all modes.
//
// The front end already relaxed the old restriction in various modes, in part
// under control of DEFAULT_PTR_TO_UNKNOWN_BOUND_ARRAY_ALLOWED_IN_PARAM_TYPE and
// DEFAULT_REF_TO_UNKNOWN_BOUND_ARRAY_ALLOWED_IN_PARAM_TYPE: Those macros are no
// longer used by the front end.
void g(double (&z)[]);  // Now allowed in all C++ modes.
