#define SOURCE_TYPE_MEMBER_DECLARATIONS_ONLY 1
#include "source_type_information_members.cpp"

namespace MemberSourceTypes {
int Owner::read(int input) const & noexcept { return value + input; }
int invoke(Owner* object)
{
    ReadMember member = &Owner::read;
    return (object->*member)(5);
}
}
