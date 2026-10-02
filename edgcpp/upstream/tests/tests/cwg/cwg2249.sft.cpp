//type:rp
//options_all:--c++17 -A
int a_ = 0;

int main()
{

/* 5141n11 expr is id only if id-expr or if part of declor-id 
   newcase 5141n11 t05b.phe 66
 */
	// _CXX17 - implements p1114r0 - 2019
	// _CXX17 - implements core 2249 - 2019
	if (::a_ != 0) 
	    return(1);
	int b = 0;
	if (a_ != 0)
		return (2);
	return(0);
	
}

//cwg: 2249
//title: identifiers and id-expressions
//meeting: Rapperswil 6/18
//edg_status: Passes
