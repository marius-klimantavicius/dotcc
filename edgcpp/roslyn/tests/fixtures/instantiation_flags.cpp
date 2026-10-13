template<class T> struct AutomaticMember { static int value; };
template<class T> int AutomaticMember<T>::value = 11;
#pragma do_not_instantiate AutomaticMember<int>::value
inline int required_inline_variable = 17;
struct RequiredInlineMember { inline static int value = 19; };
int automatic_member_use() { return AutomaticMember<double>::value; }
int automatic_old_specialization_use() { return AutomaticMember<int>::value; }
int *inline_variable_address() { return &required_inline_variable; }
int inline_member_use() { return RequiredInlineMember::value; }
