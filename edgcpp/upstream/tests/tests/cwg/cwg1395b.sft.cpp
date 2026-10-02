//type:rp
//options_all:--c++17 -tused -A

     template <class ...T> int f(T*...)    { return 1; }
     template <class T>    int f(const T&) { return 2; }
     

int main()
{
// f((int*)0) should return 1
   if (f((int*)0) == 2)
	return(2);
   return(0);
}
