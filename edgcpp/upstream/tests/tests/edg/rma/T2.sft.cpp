//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

const int ArraySize = 24; 

template <class Type>
class Array {
public: 
    Array(int sz = ArraySize); 
    Type& operator[](int index) { return ia[index]; }
protected: 
    int size;  
    Type *ia;
};

template <class Type>
Array<Type>::Array(int sz) { 
    	size = sz; ia = new Type[ size ]; 
    	for ( int i = 0; i < size; ++i ) ia[ i ] = 0; 
}

main() {
    Array<int> ia(8);	
    Array<double> p_da = Array<double>(10);

    int i;
    for ( i = 0; i < 8; ++i ) {
	ia[i] = i;	
	p_da[i] = i * 1.5;
    }
}

