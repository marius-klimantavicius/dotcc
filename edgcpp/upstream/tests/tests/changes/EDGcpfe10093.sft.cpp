//type:fp
//options_all:--gcc
//remark:[4.9] GNU: Allow zero as a valid "constructor" or "destructor" attribute value
// 2/11/14  [EDGcpfe/10093]
//
// GNU: Allow zero as a valid "constructor" or "destructor" attribute value
//
// GNU accepts (with a warning) a value of "0" for a "constructor" or "destructor"
// attribute, and now so does the front end.  Previously, the value of zero for
// the a_routine::ctor_priority and a_routine::dtor_priority fields was used as
// a "flag" to indicate that a priority value had not been specified, now two
// new fields, a_routine::has_ctor_priority and a_routine::has_dtor_priority
// have been added to fill this role.  As such, this is a slight IL CHANGE.
void f(void) __attribute__((constructor(0)));
void g(void) __attribute__((destructor(0)));
