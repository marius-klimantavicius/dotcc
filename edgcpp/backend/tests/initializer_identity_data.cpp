#include "initializer_identity.h"

IdentityFirst* identity_first;
IdentitySecond* identity_second;
int identity_array[4] = {11, 23, 47, 89};
IdentityPair identity_pair = {101, 211};
int identity_function_first() { return 13; }
int identity_function_second() { return 17; }

IdentityRow identity_rows[] = {
  {&identity_first, 8, 0},
  {&identity_second, 8, 0},
  {&identity_second, 8, 0},
  {&identity_second, 8, 0},
  {identity_array, 8, 0},
  {identity_array + 1, 8, 0},
  {identity_array + 1, 8, 0},
  {&identity_pair.first, 8, 0},
  {&identity_pair.second, 8, 0},
  {nullptr, 0, 0},
};
IdentityRow identity_repeated[] = {
  {&identity_first, 8, 19}, {&identity_first, 8, 19},
  {&identity_first, 8, 19}, {&identity_first, 8, 19},
};
IdentityFunctionRow identity_functions[] = {
  {identity_function_first, 1}, {identity_function_first, 1},
  {identity_function_second, 1},
};
IdentityFloatRow identity_floats[] = {
  {0.0, &identity_first}, {-0.0, &identity_first},
  {-0.0, &identity_first}, {-0.0, &identity_second},
};
IdentityEnvelope identity_envelopes[] = {
  {1, IdentityPayload(&identity_first)},
  {1, IdentityPayload(&identity_second)},
  {1, IdentityPayload(&identity_second)},
  {2, IdentityPayload(0.0)},
  {2, IdentityPayload(-0.0)},
  {2, IdentityPayload(-0.0)},
};
