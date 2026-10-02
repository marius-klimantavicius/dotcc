//remark:Near match on member redeclaration
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

    class V {
    public:
            template<class T> static int init(T);
    };
    class C {
            template<class T> friend void V::init(T);
            static bool booh;
    };

    template<class T> int V::init(T) {
      C::booh = true;
    }

int main() {
   V::init('0');
}

