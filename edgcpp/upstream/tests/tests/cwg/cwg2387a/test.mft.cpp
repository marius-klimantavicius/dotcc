//type: rp
//options_all: -A --c++20 -tused -e 200 --no_wrap
//source_files:cwg2387.C
//
extern "C" int printf(const char *, ...); template <class T> extern const T x; template <class T> extern T y;

const int *k = &x<int>;

int main() {
	if ( x<int> != 1)
		return(1);
	return(0);
}

//cwg: 2387
//title: Linkage of const-qualified variable template
//meeting: Kona 02/19
//edg_status: EDGcpfe/21027
//fixed_in: 6.6
