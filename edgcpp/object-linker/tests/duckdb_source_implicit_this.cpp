template<class I>I& reserve(I& it,unsigned){return it;}
template<class I>struct Writer {
 I out;
 auto reserve(unsigned n)->decltype(::reserve(out,n)){return ::reserve(out,n);}
 template<class T>int write(T value){auto&& it=reserve(1);*it++=value;return *out;}
};
int main(){int values[2]={0,9};Writer<int*> w{values};return w.write(2)==9?0:1;}
