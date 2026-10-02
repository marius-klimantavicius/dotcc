//remark:C11 _Alignof constraints
//options:--c11;fp:--c11 -A;fn:--c11 -DNEG;fn

#ifdef NEG
long l = _Alignof(int (void));
#endif
 
long k = _Alignof(int[]);
