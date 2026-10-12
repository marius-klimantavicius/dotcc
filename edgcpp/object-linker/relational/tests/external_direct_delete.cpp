#include "external_virtual_delete.h"
void external_direct_destroy(ExternalBase* pointer) { pointer->ExternalBase::~ExternalBase(); }
