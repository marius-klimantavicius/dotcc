float reciprocal(float value);
double add_double(double value);
long double add_extended(long double value);
__float128 add_quad(__float128 value);

template<double Value = 0.1> double number() { return Value; }

struct Values {
    float small;
    double wide;
    long double extended;
    __float128 quad;
};
