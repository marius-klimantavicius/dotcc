//type:fn
//options_all:--c++20 -tused
int main()
{
char *p = static_cast<char*>(operator new[](2));
p = new (p) char[2];  // #1
delete[] p;           // #2
}

//cwg: 2624
//title: Array delete expression with no array cookie
//meeting: Kona 11/22
//edg_status: Passes
