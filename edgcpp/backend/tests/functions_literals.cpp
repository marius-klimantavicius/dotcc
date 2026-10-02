int value=10;
int add(int n) { value+=n; return value; }
int twice(int n) { return n*2; }
typedef int (*Function)(int);
Function callback=add;
Function table[2]={add,twice};
const char *text="hello\0world";
const char *middle="hello\0world"+6;
const char16_t *wide=u"A\u03bb";
const char32_t* wide32=U"\U0001f680";
int invoke(Function f, int n) { return f(n); }
Function select(int n) { return n ? add : twice; }
int main() {
    if(callback!=add || table[0]!=add || select(0)!=twice) return 1;
    if(invoke(callback,3)!=13 || table[1](7)!=14 || (*select(1))(2)!=15) return 2;
    Function empty=nullptr;
    if(empty || !callback) return 3;
    unsigned long bits=reinterpret_cast<unsigned long>(callback);
    if(reinterpret_cast<Function>(bits)!=add) return 4;
    char local[]="abc";
    local[1]='x';
    if(local[0]!='a' || local[1]!='x' || local[3]!=0) return 5;
    if(text[0]!='h' || text[5]!=0 || middle[0]!='w' || text[10]!='d') return 6;
    if(wide[0]!=65 || wide[1]!=955 || wide[2]!=0) return 7;
    if(wide32[0]!=0x1f680 || wide32[1]!=0) return 8;
    return 0;
}
