//remark:C++ VLAs
//type:fp
//name:
//options:
//options_all:--g++ -x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

/* Created on 06/28/04. */
//options_all:--c99
//type:cp
int printf(const char*, ...);

void f(void* p, int m, int n)
{
	printf("%g\n", ((double (*)[m][n][1])p)[1][2][0]);
}

int main()
{
	double d[2][2][3][1] = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};

	f(d, 2, 3);
}
