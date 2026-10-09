#include "record_transport_shared.h"
int main() {
    CrossValue first = cross_make(17);
    CrossValue second{0};
    return first.value != 17 || cross_read() != 7 || !cross_nonnull(&first) ||
           cross_assign(&second, &first) != 117 || second.value != 117 ||
           first.value != 17 || cross_assignment_count() != 1;
}
