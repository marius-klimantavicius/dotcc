//remark:GNU-style bit packing
//type:rp
//name:
//options:
//options_all:--gcc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern int printf(char const*, ...);

struct __attribute__((packed)) P {
        char c:4;
        char d:8;
        char e:4;
} x;

int main() {
        printf("%d\n", sizeof(x));
        return 0;
}
