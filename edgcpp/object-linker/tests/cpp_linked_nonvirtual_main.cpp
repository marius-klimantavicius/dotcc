#include "cpp_linked_nonvirtual.h"

int main() {
    TailDerived values[2];
    if ((char*)&values[0].tail - (char*)&values[0] != 5) return 1;
    if ((char*)&values[1] - (char*)&values[0] != 8) return 2;
    TailBase source(41, 'x');
    assign_base(&values[0], source);
    if (values[0].value != 41 || values[0].marker != 'x' || values[0].tail != 'd') return 3;
    TailDerived copy = values[0];
    if (copy.value != 41 || copy.marker != 'x' || copy.tail != 'd') return 4;
    auto result = roundtrip_value(copy);
    if (result.value != 48 || result.marker != 'x' || result.tail != 'z') return 5;
    if (copy.value != 41 || copy.tail != 'd') return 6;
    TailBase complete[2];
    if ((char*)&complete[1] - (char*)&complete[0] != 8) return 7;

    Multiple multiple{};
    multiple.first = 17;
    multiple.marker = 3;
    multiple.second = 23;
    multiple.last = 31;
    PodFirst* first = &multiple;
    PodSecond* second = &multiple;
    if ((char*)second - (char*)&multiple != sizeof(PodFirst)) return 8;
    if (read_multiple(first, second) != 43 || from_second(second) != &multiple) return 9;
    if (multiple.last != 31 || from_second(nullptr) != nullptr) return 10;

    EmptyHolder empty{};
    empty.value = 43;
    if ((void*)static_cast<Empty*>(&empty) != (void*)&empty.value) return 11;
    if (sizeof(EmptyHolder) != sizeof(int)) return 12;
    RepeatedEmpty repeated{};
    repeated.EmptyHolder::value = 47;
    repeated.OtherHolder::value = 53;
    repeated.last = 59;
    if (first_empty(&repeated) == second_empty(&repeated)) return 13;
    if (repeated.EmptyHolder::value != 47 || repeated.OtherHolder::value != 53 || repeated.last != 59) return 14;

    {
        TraceDerived lifetime;
        if (trace_size != 4) return 15;
        for (int i = 0; i != 4; ++i) if (trace[i] != i + 1) return 16;
    }
    if (trace_size != 8) return 17;
    for (int i = 0; i != 4; ++i) if (trace[i + 4] != i - 4) return 18;
    return 0;
}
