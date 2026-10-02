//remark:Effect of __declspec(align(...)) on layout
//type:rp
//name:
//options:
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

__declspec(align(16))  struct A
{
  char ca ;
} ;

struct B
{
  char cb ;
  A sA ;
} ;


__declspec(align(16)) struct C
{
  char c1, c2 ;
} ;


struct D
{
  char c1 ;
  __declspec(align(16)) char c2 ;
} ;

struct E
{
  __declspec(align(16)) char c1 ;
  char c2 ;
} ;

#ifdef __cplusplus
extern "C"
#endif
int printf(char const*, ...);

   B b = { 3, 5 };
   C c = { 7, 8 } ;
   D d = { 6, 3 } ;
   E e = { 0, 1 } ;

int main()
{
   printf("B: %d [32]\n", sizeof(b));
   //assert(sizeof(b)==32) ;
   printf("D: %d [32]\n", sizeof(d));
   //assert(sizeof(d)==32) ;
   printf("C: %d [16]\n", sizeof(c));
   //assert(sizeof(c)==16) ;
   printf("E: %d [16]\n", sizeof(e));
   //assert(sizeof(e)==16) ;
}

int a1[sizeof(b)==32];
int a2[sizeof(d)==32];
int a3[sizeof(c)==16];
int a4[sizeof(e)==16];
