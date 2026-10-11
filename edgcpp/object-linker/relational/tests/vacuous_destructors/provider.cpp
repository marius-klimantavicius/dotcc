#include "program.h"

int evaluations = 0;
Scalar scalar = 7;
Empty object = {11};

Scalar* scalar_pointer() { ++evaluations; return &scalar; }
Empty* object_pointer() { ++evaluations; return &object; }
Scalar& scalar_reference() { ++evaluations; return scalar; }
Empty& object_reference() { ++evaluations; return object; }
Scalar scalar_value() { ++evaluations; return 13; }

void points_scalar() { scalar_pointer()->~Scalar(); }
void points_record() { object_pointer()->~Empty(); }
void dot_scalar() { scalar_reference().~Scalar(); }
void dot_record() { object_reference().~Empty(); }
void dot_field() { object_reference().value.~Scalar(); }
void dot_value() { scalar_value().~Scalar(); }
