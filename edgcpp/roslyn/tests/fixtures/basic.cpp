#define SUM(x,y) ((x)+(y))
int add(int a,int b) { return SUM(a,b); }
int main() { int x=add(2,3); if(x>0) x+=1; return x; }
