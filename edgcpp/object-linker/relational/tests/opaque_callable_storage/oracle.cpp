#include "result.h"
int main() {
    Holder first{};
    Holder second{};
    if (!is_empty(&first)) return 1;
    set_callback(&first, bound_factory());
    if (get_callback(&first) != &make_result) return 2;
    copy_holder(&second, &first);
    clear_holder(&first);
    if (!is_empty(&first) || is_empty(&second)) return 3;
    if (get_callback(&second) != bound_factory()) return 4;
    // The native oracle has the complete result declaration; generated
    // opaque storage is deliberately never invoked through its callback.
    if (make_result().value != 29) return 5;
    return 0;
}
