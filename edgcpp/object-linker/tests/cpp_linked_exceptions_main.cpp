#include "cpp_linked_exceptions.h"

namespace { struct Local { int value; }; }

int main() {
    try { bridge_raise(0); } catch (long long) { return 1; }
    catch (long value) { if (value != 101) return 2; }
    try { bridge_raise(1); } catch (long) { return 3; }
    catch (long long value) { if (value != 103) return 4; }
    try { bridge_raise(2); } catch (signed char) { return 5; }
    catch (char value) { if (value != 41) return 6; }
    try { bridge_raise(3); } catch (char) { return 7; }
    catch (signed char value) { if (value != 43) return 8; }
    try { bridge_raise(4); } catch (second::Value) { return 9; }
    catch (const first::Value& value) { if (value.value != 47) return 10; }
    try { bridge_raise(5); } catch (first::Value) { return 11; }
    catch (const second::Value& value) { if (value.value != 53) return 12; }
    try { bridge_raise(6); } catch (second::Code) { return 13; }
    catch (first::Code value) { if (value != first::Code::value) return 14; }
    try { bridge_raise(7); } catch (int) { return 15; }
    catch (first::Code) { return 16; }
    catch (second::Code value) { if (value != second::Code::value) return 17; }
    try { bridge_raise(8); }
    catch (Right& value) {
        if (value.right != 29) return 18;
        auto complete = dynamic_cast<Payload*>(&value);
        if (!complete || complete->value != 77 || complete->left != 11) return 19;
        if (payload_destructions != 0) return 20;
    }
    if (payload_destructions != 1) return 21;
    try { bridge_raise(9); }
    catch (Payload* value) { if (value != payload_address()) return 22; }
    try { bridge_raise(10); }
    catch (const int* value) { if (value != nullptr) return 23; }
    try { bridge_raise(11); }
    catch (OrdinaryFunction value) { if (value() != 19) return 24; }
    try { bridge_raise(12); } catch (NoexceptFunction) { return 25; }
    catch (OrdinaryFunction value) { if (value() != 31) return 26; }
    try { bridge_raise(13); } catch (Local) { return 27; }
    catch (...) {}
    try { bridge_raise(14); } catch (const int**) { return 28; }
    catch (const int* const* value) { if (**value != 61) return 29; }
    if (bridge_destructions != 15) return 30;
    Payload* complete = payload_address();
    Right* right = cast_right(complete);
    if (right != static_cast<Right*>(complete) || right->right != 29) return 31;
    if (cast_complete(right) != complete || cast_right(nullptr) != nullptr) return 32;
    return 0;
}
