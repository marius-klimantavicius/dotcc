//remark:Generated exception specifications
//type:fn
//name:
//options:-A;fn:;fp
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct A{};
struct B{};


struct C {
        virtual ~C() throw (A) {}
};
struct D {
        virtual ~D() throw (B) {}
};


struct E : C, D {};


int
main()
{
        E e;
}
