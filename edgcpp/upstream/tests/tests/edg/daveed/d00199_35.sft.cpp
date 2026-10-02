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


	void f()
	{
		int n = 10;
		try
		{
		}
		catch (int x[n])
		{
		}
	}
