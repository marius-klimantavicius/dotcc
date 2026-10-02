//remark:GNU case ranges
//type:fp
//name:
//options:--gnu_version=40200 -tused:-DNEG -tused;fn:;fn
//options_all:-tused
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

template<int I, int J> void f(int i) {
	switch (i) {
		case 1 ... I:
		case J ... 10:
			break;
	}
}

int main() {
	f<4, 5>(3);
#ifdef NEG
	f<5, 5>(3);
#endif
}
