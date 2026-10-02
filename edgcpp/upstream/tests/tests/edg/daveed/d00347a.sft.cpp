//remark:Static data member specialization
//type:fp
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


template<int> struct A {
    static char table[];
};

template<> char A<0>::table[] = { 0 };
template<> char A<1>::table[1000] = { 0 };

int a[1/(sizeof(A<1>::table) == 1000)];
