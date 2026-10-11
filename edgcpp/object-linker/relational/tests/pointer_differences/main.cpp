#include "program.h"
int main() {
    char bytes[9] = {};
    if (char_distance(bytes + 8, bytes + 2) != 6) return 1;
    if (char_distance(bytes + 2, bytes + 8) != -6) return 2;
    if (char_distance(bytes + 3, bytes + 3) != 0) return 3;
    int words[8] = {};
    if (int_distance(words + 7, words + 2) != 5) return 4;
    if (int_distance(words + 2, words + 7) != -5) return 5;
    if (int_distance(words + 4, words + 4) != 0) return 6;
    int left_calls = 0;
    int right_calls = 0;
    int state = 0;
    Difference result = 0;
    *selected_destination(&result, &left_calls, &right_calls, &state) =
        selected_left(words + 6, &left_calls) - selected_right(words + 1, &right_calls);
    if (result != 5 || left_calls != 1 || right_calls != 1 || state != 7) return 7;
    left_calls = 0;
    right_calls = 0;
    Difference conditional = false ? selected_left(words + 5, &left_calls) - selected_right(words, &right_calls) : -3;
    if (conditional != -3 || left_calls != 0 || right_calls != 0) return 8;
    if (left_calls != 0 && selected_left(words + 5, &left_calls) - selected_right(words, &right_calls) > 0) return 9;
    if (left_calls != 0 || right_calls != 0) return 10;
    return 0;
}
