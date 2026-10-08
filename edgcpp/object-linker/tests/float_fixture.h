#ifndef FLOAT_FIXTURE_H
#define FLOAT_FIXTURE_H
#ifdef __cplusplus
extern "C" {
#endif
float float_add(float left, float right);
double double_add(double left, double right);
float float_identity(float value);
double double_identity(double value);
double float_array_sum(const float values[], int count);
void double_store(double* target, double value);
#ifdef __cplusplus
}
#endif
#endif
