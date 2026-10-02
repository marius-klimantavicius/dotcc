//type: rp
//options_all: -A --c++20 -tused -e 200 --no_wrap
//source_files:cwg2387.C
//
extern "C" int printf(const char *, ...); template <class T> extern const T x; template <class T> extern T y;

const int *k = &x<int>;

int main() {
	if ( y<int> != 1)
		return(1);
	return(0);
}

