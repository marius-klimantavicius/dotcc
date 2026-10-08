#include "array_fixture.h"

int array_next(int* counter) { return ++*counter; }

int array_checks(void)
{
    int values[5] = {7, 11};
    int inferred[] = {3, 5, 7};
    const int immutable[2] = {41, 43};
    if (sizeof(inferred) != 12 || array_sum(inferred, 3) != 15 || array_sum(immutable, 2) != 84) return 26;
    if (values[0] != 7 || values[1] != 11 || values[2] || values[3] || values[4]) return 1;
    if (array_sum(values, 5) != 18) return 2;
    int row[3] = {1, 2, 3};
    if (array_sum_three(row) != 6) return 3;
    array_bump_row(&row);
    if (row[1] != 7 || sizeof(row) != 12) return 4;
    int matrix[2][3] = {{2, 3}, {5, 7, 11}};
    if (matrix[0][2] || array_sum_matrix(matrix, 2) != 28) return 5;
    int (*rows)[3] = matrix;
    if (rows + 1 != &matrix[1] || rows[1][2] != 11) return 6;
    if ((&matrix + 1) - &matrix != 1 || sizeof(matrix) != 24) return 7;
    int (*absent_row)[3] = 0;
    if (absent_row || array_sum_typedef(row) != 11 || __DotObjArray_A3_i32() != 41) return 21;
    int cube[2][2][2] = {1, 2, 3, 4, 5};
    if (cube[0][1][1] != 4 || cube[1][0][0] != 5 || cube[1][1][1]) return 22;
    int* pointers[3] = {&values[0], &row[2], 0};
    if (*pointers[0] != 7 || *pointers[1] != 3 || pointers[2]) return 8;
    ++*pointers[0];
    if (values[0] != 8) return 9;

    int counter = 0;
    int dynamic[3] = {array_next(&counter), array_next(&counter), array_next(&counter)};
    if (counter != 3 || array_sum(dynamic, 3) != 6) return 10;
#ifdef __cplusplus
    if (dynamic[0] != 1 || dynamic[1] != 2 || dynamic[2] != 3) return 11;
    bool flags[3] = {true, false};
    if (!flags[0] || flags[1] || flags[2]) return 23;
#endif
    int nested_dynamic[2][2] = {{array_next(&counter)}, {array_next(&counter)}};
    if (counter != 5 || nested_dynamic[0][1] || nested_dynamic[1][1] || nested_dynamic[0][0] + nested_dynamic[1][0] != 9) return 24;
    int total = 0;
    for (int i = 0; i < 4; ++i) {
        int repeated[3] = {i, 2};
        if (repeated[2]) return 12;
        total = total + array_sum_three(repeated);
        repeated[2] = 99;
    }
    if (total != 14) return 13;
    {
        int values[2] = {31, 37};
        if (array_sum(values, 2) != 68) return 14;
    }
    if (values[0] != 8) return 15;
    unsigned long long wide[3] = {0ULL, 9223372036854775808ULL, 18446744073709551615ULL};
    if (wide[1] != 9223372036854775808ULL || wide[2] != 18446744073709551615ULL) return 16;
    unsigned char bytes[4] = {255, 0, 17};
    if (++bytes[0] || bytes[3] || bytes[2] != 17) return 17;
    char text[7] = "a\0b";
    if (text[0] != 'a' || text[1] || text[2] != 'b' || text[3] || text[6]) return 18;
#ifndef __cplusplus
    char exact[3] = "abc";
    if (exact[0] != 'a' || exact[2] != 'c') return 25;
    int designated[6] = {[4] = 9, [1] = 7};
    if (designated[0] || designated[1] != 7 || designated[2] || designated[3] || designated[4] != 9 || designated[5]) return 19;
#endif
    return 0;
}

int array_boundary(void)
{
    unsigned char large[65536] = {17};
    if (large[0] != 17 || large[65535]) return 20;
    large[65535] = 23;
    return large[65535] - 23;
}

int main(void)
{
    int result = array_checks();
    if (result) return result;
    return array_boundary();
}
