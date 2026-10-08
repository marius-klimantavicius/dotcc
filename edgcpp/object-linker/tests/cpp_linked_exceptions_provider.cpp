#include "cpp_linked_exceptions.h"

int payload_destructions;
int bridge_destructions;
Left::~Left() = default;
Right::~Right() = default;
Payload::~Payload() { ++payload_destructions; }
int invoke_noexcept() noexcept { return 19; }
int invoke_ordinary() { return 31; }
Payload* payload_address() {
    static Payload payload;
    return &payload;
}

namespace { struct Local { int value; }; }

void raise_value(int mode) {
    switch (mode) {
    case 0: throw static_cast<long>(101);
    case 1: throw static_cast<long long>(103);
    case 2: throw static_cast<char>(41);
    case 3: throw static_cast<signed char>(43);
    case 4: throw first::Value{47};
    case 5: throw second::Value{53};
    case 6: throw first::Code::value;
    case 7: throw second::Code::value;
    case 8: throw Payload{};
    case 9: throw payload_address();
    case 10: throw nullptr;
    case 11: throw &invoke_noexcept;
    case 12: throw &invoke_ordinary;
    case 13: throw Local{59};
    case 14: {
        static int value = 61;
        static int* pointer = &value;
        throw &pointer;
    }
    default: throw -1;
    }
}

Right* cast_right(Left* value) { return dynamic_cast<Right*>(value); }
void* cast_complete(Right* value) { return dynamic_cast<void*>(value); }
