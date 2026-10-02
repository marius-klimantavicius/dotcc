//file
template <class T> struct A {};

template <> struct A<double> {
	static A<double> x;
};

