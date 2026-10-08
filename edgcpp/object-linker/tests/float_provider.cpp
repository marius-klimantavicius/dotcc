#include "float_fixture.h"
float float_add(float left, float right) { return left + right; }
double double_add(double left, double right) { return left + right; }
float float_identity(float value) { return value; }
double double_identity(double value) { return value; }
double float_array_sum(const float values[], int count)
{
    double sum = 0.0;
    for (int i = 0; i < count; ++i) sum = sum + values[i];
    return sum;
}
void double_store(double* target, double value) { *target = value; }
