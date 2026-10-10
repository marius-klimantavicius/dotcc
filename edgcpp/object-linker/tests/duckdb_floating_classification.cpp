// No library classifications: preserve the compiler builtin's actual operand width.
union F32 { unsigned bits; float value; };
union F64 { unsigned long bits; double value; };
union F80 { struct { unsigned long significand; unsigned short sign_exponent; } bits; long double value; };
static_assert(sizeof(float)==4 && sizeof(double)==8 && sizeof(long double)==16);

int generic(float x) {
    return !!__builtin_signbit(x) | (!!__builtin_isnan(x)<<1) |
           (!!__builtin_isinf(x)<<2) | (!!__builtin_isfinite(x)<<3);
}
int generic(double x) {
    return !!__builtin_signbit(x) | (!!__builtin_isnan(x)<<1) |
           (!!__builtin_isinf(x)<<2) | (!!__builtin_isfinite(x)<<3);
}
int generic(long double x) {
    return !!__builtin_signbit(x) | (!!__builtin_isnan(x)<<1) |
           (!!__builtin_isinf(x)<<2) | (!!__builtin_isfinite(x)<<3);
}
int specific(float x) {
    return !!__builtin_signbitf(x) | (!!__builtin_isnanf(x)<<1) |
           (!!__builtin_isinff(x)<<2) | (!!__builtin_isfinite(x)<<3);
}
int specific(long double x) {
    return !!__builtin_signbitl(x) | (!!__builtin_isnanl(x)<<1) |
           (!!__builtin_isinfl(x)<<2) | (!!__builtin_isfinite(x)<<3);
}
int calls;
float once(float x) { ++calls; return x; }
long double once(long double x) { ++calls; return x; }
double ordinary_variadic(int tag, ...) {
    __builtin_va_list args;
    __builtin_va_start(args,tag);
    double result=__builtin_va_arg(args,double);
    __builtin_va_end(args);
    return result;
}
double check_promotion(float x) { return ordinary_variadic(0,x); }

int main() {
    unsigned fbits[]={0,0x80000000U,1,0x80000001U,0x3f800000U,0xbf800000U,
                      0x7f800000U,0xff800000U,0x7fc00001U,0xffc00001U,0x7f800001U};
    unsigned long dbits[]={0,0x8000000000000000UL,1,0x8000000000000001UL,
        0x3ff0000000000000UL,0xbff0000000000000UL,0x7ff0000000000000UL,
        0xfff0000000000000UL,0x7ff8000000000001UL,0xfff8000000000001UL,0x7ff0000000000001UL};
    unsigned long lm[]={0,0,1,1,0x8000000000000000UL,0x8000000000000000UL,
        0x8000000000000000UL,0x8000000000000000UL,0xc000000000000001UL,
        0xc000000000000001UL,0x8000000000000001UL};
    unsigned short le[]={0,0x8000,0,0x8000,0x3fff,0xbfff,0x7fff,0xffff,0x7fff,0xffff,0x7fff};
    int expected[]={8,9,8,9,8,9,4,5,2,3,2};
    for(int i=0;i!=11;++i) {
        F32 f; f.bits=fbits[i]; F64 d; d.bits=dbits[i]; F80 l={};
        l.bits.significand=lm[i]; l.bits.sign_exponent=le[i];
        if(generic(f.value)!=expected[i] || specific(f.value)!=expected[i])return 10+i;
        if(generic(d.value)!=expected[i])return 30+i;
        if(generic(l.value)!=expected[i] || specific(l.value)!=expected[i])return 50+i;
    }
    // Finite binary80 values beyond binary64's range must stay finite.
    F80 huge={}; huge.bits.significand=0x8000000000000000UL; huge.bits.sign_exponent=0x7ffe;
    if(generic(huge.value)!=8 || specific(huge.value)!=8)return 70;
    calls=0; F32 n; n.bits=0xffc00001U;
    if(!__builtin_signbit(once(n.value)) || !__builtin_isnan(once(n.value)) ||
       __builtin_isinf(once(n.value)) || __builtin_isfinite(once(n.value)) || calls!=4)return 71;
    calls=0;
    if(__builtin_signbit(once(huge.value)) || __builtin_isnan(once(huge.value)) ||
       __builtin_isinf(once(huge.value)) || !__builtin_isfinite(once(huge.value)) || calls!=4)return 72;
    if(check_promotion(1.5f)!=1.5)return 73;
    return 0;
}
