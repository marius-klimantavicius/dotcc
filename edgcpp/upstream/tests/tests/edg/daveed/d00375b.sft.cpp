//remark:GNU template attributes
//type:fn
//name:
//options:
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template<class T> struct __attribute__((aligned(sizeof(int)))) S;
template<class T> struct __attribute__((aligned(sizeof(T)))) S {
	int i;
};


S<char> s;

extern "C" int printf(char const*, ...);
int main() {
	printf("%d\n", __alignof__(s));
}

template<class T> struct __attribute__((aligned(sizeof(T)))) S {
	int i;
};
