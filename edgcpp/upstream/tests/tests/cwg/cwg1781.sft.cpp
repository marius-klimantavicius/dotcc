//type:fp
//options_all:--c++17 -A
namespace std {
typedef decltype(nullptr) nullptr_t;}
struct A_ {
	operator std:: nullptr_t() { return false; }
};
bool b { A_() };

int main()
{

/* _13332n4a convering from nullptr_t to bool to overload resolution 
   newcase _13332n4a t13b.phe 1018
 */
	{
	// _CXX17 - Implements p1350r0 - in C++17 status 2019
	// _CXX17 - Implements core 1781 - in C++17 status 2019
	// _CXX17 - Implements core 2133 - in C++17 status 2019
	// _CXX17 - Implements core 2243 - in C++17 status 2019
	
	}
}

//cwg: 1781
//title: Converting from nullptr_t to bool in overload resolution
//meeting: San Diego 11/18
//edg_status: EDGcpfe/21901
