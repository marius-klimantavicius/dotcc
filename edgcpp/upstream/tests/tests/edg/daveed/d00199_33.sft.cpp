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

int main()
{
	int i = 37;
	int j = sizeof(int[(int){i}]);

	if (j != sizeof(int[37]))
		return 1;

	return 0;
}

