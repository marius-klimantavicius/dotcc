#include "source_type_information_c_api.h"
int main(void)
{
    struct CEnumCatalog value = { AnonymousHigh, NamedHigh };
    return read_c_enum(&value) == 15 ? 0 : 1;
}
