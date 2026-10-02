//remark:Error recovery for strange declarations
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



template < class T > struct T { 
  friend int main ( A < T > ) { } 
  friend int main ( T < int > ) { } 
}; 

