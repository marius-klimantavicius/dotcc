//remark:GNU C++ attributes on function templates
//type:fp
//name:
//options:
//options_all:--g++ -tused
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template<typename T> void f() __attribute__((const));

template<typename T> void f() {}

template void f<double>();

#if 1
template <class T> struct complex {};
template <class _FLT> inline _FLT
imag (const complex<_FLT>& x) __attribute__ ((const));
#endif
