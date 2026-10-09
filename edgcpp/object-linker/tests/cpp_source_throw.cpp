// Owned throw/rethrow generation fixture. Runtime exception binding is separate.
static int evaluations, copies, destructions;
using IntegerAlias = int;
struct Empty {};
struct Payload {
    int value;
    explicit Payload(int input) : value(input) {}
    Payload(const Payload &other) : value(other.value) { ++copies; }
    ~Payload() noexcept(false) { ++destructions; }
};
struct Prefix { int prefix; };
struct Base { int value; };
struct Derived : Prefix, Base {};
struct PrivateDerived : private Base {};
struct Left : Base {};
struct Right : Base {};
struct Ambiguous : Left, Right {};
extern "C" void throw_integer(IntegerAlias value) { throw (++evaluations, value); }
extern "C" void throw_long(long value) { throw value; }
extern "C" void throw_long_long(long long value) { throw value; }
extern "C" void throw_float(float value) { throw value; }
extern "C" void throw_pointer(int *value) { throw value; }
extern "C" void throw_const_pointer(const int *value) { throw value; }
extern "C" void throw_pointer_chain(const int *const *value) { throw value; }
extern "C" void throw_void_pointer(void *value) { throw value; }
extern "C" void throw_empty() { throw Empty(); }
extern "C" void throw_payload(int value) { throw Payload(value); }
extern "C" void throw_payload_copy(const Payload *value) { throw *value; }
extern "C" void throw_derived_pointer(Derived *value) { throw value; }
extern "C" void throw_private_pointer(PrivateDerived *value) { throw value; }
extern "C" void throw_ambiguous_pointer(Ambiguous *value) { throw value; }
extern "C" void rethrow_current() { throw; }
extern "C" int conditional_throw(int flag, int value) { return flag ? value : throw 41; }
extern "C" Base conditional_record_throw(int flag, Base value) { return flag ? value : throw 43; }
extern "C" int nonvoid_throw() { throw 47; }
