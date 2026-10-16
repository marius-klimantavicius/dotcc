#ifndef ROSLYNOBJ_POINTER_CALL_CHAIN_HPP
#define ROSLYNOBJ_POINTER_CALL_CHAIN_HPP

// Native oracle inputs are live C++ objects. Captured code only borrows them
// and calls the nonvirtual reader; it never constructs, destroys or dispatches
// virtually through a CellInput. Generated tests will supply aligned layout
// bytes instead and make no corresponding C++ lifetime claim.
enum class FieldCode : unsigned char {
    first = 5,
    second = 201,
    changed_first = 37,
    changed_second = 19
};

struct InputPrefix {
    long long first;
    long long second;
};

struct CellInput : InputPrefix {
    virtual int unused() const = 0;
    FieldCode kind;
    long long tail[3];

    FieldCode read() const;
};

extern "C" int fixture(CellInput* first, CellInput* second);
extern "C" int lifetime_count();
extern "C" int lifetime_overflow();
// This zero-valued slot is the scalar harness protocol, not ownership evidence.
extern "C" int lifetime_live();
extern "C" int lifetime_event(int index);
extern "C" int lifetime_value(int index);

#endif
