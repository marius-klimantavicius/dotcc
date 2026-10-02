//remark:Redeclaration of function defined in friend of misformed class
//type:fn
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template < class T > struct class { 
        friend int g(int i) {return i;}
};

int g(int);

