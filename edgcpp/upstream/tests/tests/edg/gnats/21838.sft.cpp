//type:fn
//options_all:--microsoft_v 1924
template<typename T>
void f(T *buffer, int size, int &size_read);
 
template<typename T, int Size>
void f(T(&buffer)[Size], int &Size)
{
              return f(buffer, Size, Size);
}
