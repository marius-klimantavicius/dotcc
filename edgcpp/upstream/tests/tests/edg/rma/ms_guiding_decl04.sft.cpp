//options_all:-r -x -tused
//options: --microsoft;cp

template <class T> 
class C 
{
    friend void operator-(const C<T>& x, const C<T>& y);
};

template <class T> inline void operator- (const C<T>& x, const C<T>& y) {;}

int main ()
{
    C<int> r;
    r - r;
    return 0;
}

