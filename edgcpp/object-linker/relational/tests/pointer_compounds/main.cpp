#include "program.h"
int main() {
    unsigned char bytes[16] = {};
    if (shift_bytes(bytes + 4, 3, false) != bytes + 7) return 1;
    if (shift_bytes(bytes + 4, -3, false) != bytes + 1) return 2;
    if (shift_bytes(bytes + 4, 0, false) != bytes + 4) return 3;
    if (shift_bytes(bytes + 4, 3, true) != bytes + 1) return 4;
    if (shift_bytes(bytes + 4, -3, true) != bytes + 7) return 5;
    if (shift_bytes(bytes + 4, 0, true) != bytes + 4) return 6;
    int words[16] = {};
    if (shift_words(words + 4, 3, false) != words + 7) return 7;
    if (shift_words(words + 4, -3, false) != words + 1) return 8;
    if (shift_words(words + 4, 0, false) != words + 4) return 9;
    if (shift_words(words + 4, 3, true) != words + 1) return 10;
    if (shift_words(words + 4, -3, true) != words + 7) return 11;
    if (shift_words(words + 4, 0, true) != words + 4) return 12;
    int *pointer = words;
    int trace = 0;
    int target_calls = 0;
    int offset_calls = 0;
    int **address = &(*selected_target(&pointer, &trace, &target_calls) +=
        selected_offset(3, &trace, &offset_calls));
    if (address != &pointer || pointer != words + 3 || trace != 12 ||
        target_calls != 1 || offset_calls != 1) return 13;
    trace = 0;
    target_calls = 0;
    offset_calls = 0;
    pointer = words;
    address = &((*selected_target(&pointer, &trace, &target_calls) +=
        selected_offset(4, &trace, &offset_calls)) -= selected_offset(1, &trace, &offset_calls));
    if (address != &pointer || pointer != words + 3 || trace != 112 ||
        target_calls != 1 || offset_calls != 2) return 14;
    int *&reference = (pointer += 1);
    if (&reference != &pointer || pointer != words + 4) return 15;
    reference = words + 2;
    if (pointer != words + 2) return 16;
    (pointer -= 0) = words + 6;
    if (pointer != words + 6) return 17;
    (pointer += 1) += 2U;
    if (pointer != words + 9) return 18;
    trace = 0;
    target_calls = 0;
    offset_calls = 0;
    if (trace != 0 && (*selected_target(&pointer, &trace, &target_calls) +=
        selected_offset(1, &trace, &offset_calls))) return 19;
    if (target_calls != 0 || offset_calls != 0 || pointer != words + 9) return 20;
    int *chosen = trace == 0 ? (pointer -= 2) :
        (*selected_target(&pointer, &trace, &target_calls) += selected_offset(1, &trace, &offset_calls));
    if (chosen != words + 7 || pointer != chosen || target_calls != 0 || offset_calls != 0) return 21;
    shared_pointer = words;
    address = &(shared_pointer += 2);
    if (address != &shared_pointer || shared_pointer != words + 2) return 22;
    *address -= 1;
    if (shared_pointer != words + 1) return 23;
    return 0;
}
