//type:fn
//options:--microsoft_version 1300:--guiding --microsoft_version 1300:--microsoft_version 1936:-A
//options_all:--c++

template<typename T>
int f(T, T)
{ return 1; }

int f(int, int);
int f(const int &, const int &);

// Note that MSVC 13.00 doesn't generate an error here, but we just test that
// we don't assert.
int i = f(1, 1);
