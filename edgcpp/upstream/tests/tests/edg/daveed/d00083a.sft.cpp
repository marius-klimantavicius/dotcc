//remark:Error recovery in templates
//type:fn
//name:
//options:
//options_all:-A
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template < class T , template < class _T > class U > class ++ { 
#pragma weak
        friend class T::template C< U<T> >;
};
