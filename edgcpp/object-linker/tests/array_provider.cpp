#include "array_fixture.h"
int array_sum(const int values[], int count)
{
    int total = 0;
    for (int i = 0; i < count; ++i) total = total + values[i];
    return total;
}
int array_sum_three(const int values[3]) { return array_sum(values, 3); }
int array_sum_matrix(int values[][3], int rows)
{
    int total = 0;
    for (int i = 0; i < rows; ++i) total = total + array_sum(values[i], 3);
    return total;
}
void array_bump_row(int (*row)[3]) { (*row)[1] = (*row)[1] + 5; }
int array_sum_typedef(ArrayRow values) { return array_sum(values, 3); }
int __DotObjArray_A3_i32(void) { return 41; }
