/* Actual TR 18037 literals and type descriptors; run --c99 --fixed_point. */
short _Fract short_fraction = 0.25hr;
_Fract fraction = -0.5r;
long _Fract long_fraction = 0.125lr;
unsigned _Fract unsigned_fraction = 0.75ur;
_Accum accumulator = 2.25k;
unsigned long _Accum unsigned_accumulator = 3.5ulk;
_Sat _Accum saturated_accumulator = 1.5k;
int fixed_storage_size = sizeof(_Accum);
int fixed_fraction_size = sizeof(short _Fract);
_Accum fixed_sum(_Accum value) { return value + accumulator; }
