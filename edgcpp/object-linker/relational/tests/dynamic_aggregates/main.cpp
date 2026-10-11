#include "program.h"
int main() {
    Pair first{mark(3), mark(4)};
    if (first.first != 3 || first.second != 4 || calls != 2) return 1;
    Pair partial{mark(5)};
    if (partial.first != 5 || partial.second != 0 || calls != 3) return 2;
    Pair returned = make_pair(6);
    if (returned.first != 6 || returned.second != 7 || calls != 5) return 3;
    Nested nested{{mark(8), mark(9)}, mark(10)};
    if (nested.pair.first != 8 || nested.pair.second != 9 || nested.tail != 10 || calls != 8) return 4;
    int array[4] = {mark(11), mark(12)};
    if (array[0] != 11 || array[1] != 12 || array[2] != 0 || array[3] != 0 || calls != 10) return 5;
    int matrix[2][2] = {{mark(13), mark(14)}, {mark(15)}};
    if (matrix[0][0] != 13 || matrix[0][1] != 14 || matrix[1][0] != 15 || matrix[1][1] != 0 || calls != 13) return 6;
    View view = make_view(array, 4);
    view.data[1] = 19;
    if (view.size != 4 || array[1] != 19) return 7;
    try { Pair failed{mark(16), mark(-1)}; return failed.first; } catch (int error) {
        if (error != 17 || calls != 15) return 8;
    }
    return 0;
}
