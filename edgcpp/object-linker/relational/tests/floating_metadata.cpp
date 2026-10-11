float source_float(float);
double source_double(double);
long double source_long_double(long double);
__float128 source_float128(__float128);

int main() {
    return sizeof(float) == 4 && sizeof(double) == 8 && sizeof(long double) == 16 ? 0 : 1;
}
