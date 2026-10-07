// Two-TU relocation and representation identity fixture.
struct IdentityFirst { long value; };
struct IdentitySecond { unsigned long value; };
struct IdentityPair { int first, second; };
struct IdentityRow { const void* address; unsigned long size; unsigned flags; };
struct IdentityFunctionRow { int (*function)(); int tag; };
struct IdentityFloatRow { double value; const void* address; };
union IdentityPayload {
  const void* address;
  double value;
  constexpr IdentityPayload(const void* p) : address(p) {}
  constexpr IdentityPayload(double v) : value(v) {}
};
struct IdentityEnvelope { int tag; IdentityPayload payload; };

extern IdentityFirst* identity_first;
extern IdentitySecond* identity_second;
extern int identity_array[4];
extern IdentityPair identity_pair;
extern IdentityRow identity_rows[10];
extern IdentityRow identity_repeated[4];
extern IdentityFunctionRow identity_functions[3];
extern IdentityFloatRow identity_floats[4];
extern IdentityEnvelope identity_envelopes[6];
int identity_function_first();
int identity_function_second();
