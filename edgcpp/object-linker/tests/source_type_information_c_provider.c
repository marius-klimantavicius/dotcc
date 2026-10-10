#include "source_type_information_c_api.h"
int read_c_enum(const struct CEnumCatalog* value) { return value->value + value->named; }
