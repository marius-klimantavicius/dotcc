//remark:GNU C _Complex/__complex without fp type
//options:--gcc;cp:--g++;fn:--gcc -DNEG;fn

_Complex value = 1.0+2.0i;
__complex val = 1.0+2.0i;

#ifdef NEG
_Imaginary ii = 1.0i;
#endif
