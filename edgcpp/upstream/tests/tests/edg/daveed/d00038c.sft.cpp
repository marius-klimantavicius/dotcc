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

void f() throw();
void (*f)() throw(int);

void (*g)() throw(int);
void g() throw();

