extern "C" void _Exit(int);
int alive=0, destroyed=0, owners=0;
struct Report {~Report(){if(alive!=0 || destroyed!=5 || owners!=0)_Exit(70);}} report;
struct A {int value; A(int n):value(n){++alive;} ~A(){--alive;++destroyed;}};
struct B {int sum; B(const A& left,const A& right):sum(left.value+right.value){++owners;} ~B(){--owners;}};
A first(1);
B second(A(2),A(3));
B third(A(4),A(5));
int main(){return alive==1 && destroyed==4 && owners==2 && second.sum==5 && third.sum==9 ? 0 : 1;}
