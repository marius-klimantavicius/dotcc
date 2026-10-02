//remark:GNU case ranges
//type:fp
//name:
//options:--gnu_version=30300 -tused;fn:--gnu_version=40200:--parse;fn
//options_all:
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
		case I ... J:
		case I+2 ... J-2:
			break;
	}
}

int main() {
	f<1, 11>(1);
}

