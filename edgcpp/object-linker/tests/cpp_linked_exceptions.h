// Independent-TU exception identity must survive physical ABI canonicalization.
#pragma once
namespace first { struct Value { int value; }; enum Code { value = 23 }; }
namespace second { struct Value { int value; }; enum Code { value = 23 }; }

struct Left {
    int left = 11;
    virtual ~Left();
};
struct Right {
    int right = 29;
    virtual ~Right();
};
struct Payload : Left, Right {
    int value = 77;
    ~Payload() override;
};

using OrdinaryFunction = int (*)();
using NoexceptFunction = int (*)() noexcept;
extern int payload_destructions;
extern int bridge_destructions;
void raise_value(int mode);
void bridge_raise(int mode);
Payload* payload_address();
int invoke_noexcept() noexcept;
int invoke_ordinary();
Right* cast_right(Left*);
void* cast_complete(Right*);
