//remark:Match exception specifications on variables
//type:fn
//name:
//options:
//options_all:-x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template<typename T>
struct X {
	static void (X::*pmd)() throw();
};

template<typename T> void (X<T>::*X<T>::pmd)() throw (bool);

void f() {
	X<void>::pmd = 0;
}

