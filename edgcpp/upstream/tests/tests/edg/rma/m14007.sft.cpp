//options_all:-r -x -tused
//options: --strict;cn

// #007 _141p18 the decl must decl or define a fn or class

template <class T>
	template <class U>         // error - template decl's template
		class x { T i; U j; };

