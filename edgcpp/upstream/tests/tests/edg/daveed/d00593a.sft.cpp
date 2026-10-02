//remark:Microsoft __cdecl
//type:fn
//options_all:-r --microsoft_version=1310 -d-dump_symbols
void __cdecl f1(short __cdecl i);
void __cdecl f1(long __cdecl j);
void f1(__cdecl *);
void f1(char *const);

int main()
{
	f1(0);
}
