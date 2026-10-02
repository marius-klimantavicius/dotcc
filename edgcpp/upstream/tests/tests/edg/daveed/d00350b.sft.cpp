//remark:Microsoft for-init scope
//type:rp
//name:
//options:--microsoft_version=1310;fp:--microsoft_version=1300;fp
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#define assert(x) printf("%s: %d\n", #x, x);

extern "C" int printf(char const*, ...);

 void f() {     
	int i = 0 ;
      {
        for (int i=0; i<10; i++)
          ;
        assert(i==10) ;
        for (int i=0; i<5; i++)
          ;
        assert(i==5) ;
      }
      assert(i==0) ;
}
