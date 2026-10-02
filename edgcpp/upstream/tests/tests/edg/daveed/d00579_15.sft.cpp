//remark:GNU C/C++ complex type support
//type:fp
//name:
//options:--gcc;fn:--g++
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

_Complex double const pi =
 3.14
#ifdef __cplusplus
 +0.0i
#endif
;

double pi2 = __real pi;

#ifdef __cplusplus

template<class T> T f(T x) {
	return __real x;
}


#endif

