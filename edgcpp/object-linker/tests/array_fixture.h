#ifndef ARRAY_FIXTURE_H
#define ARRAY_FIXTURE_H
typedef int ArrayRow[3];
#ifdef __cplusplus
extern "C" {
#endif
int array_sum(const int values[], int count);
int array_sum_three(const int values[3]);
int array_sum_matrix(int values[][3], int rows);
void array_bump_row(int (*row)[3]);
int array_sum_typedef(ArrayRow values);
int __DotObjArray_A3_i32(void);
#ifdef __cplusplus
}
#endif
#endif
