//options_all:-r -x -tused
//options: --strict;cn

// #039 _146p21: The template args for a template class member fct are
	// determined by the template args of the type of the object for
	// which the fct is called

template<class T> struct X
	{
	T i;
	X(T ii) : i(ii) { }
	T foo() { return i; }
	};
template<class T> X<T>::X<T>(T ii) : i(ii) { }
	// error - template arg implied, can't be stated
int main()
	{
	return X(0).i;
	}

