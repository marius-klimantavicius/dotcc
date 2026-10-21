constexpr int gate(){
 int total=0;
 for(int n=3;int v=n;n=v-1) total+=v;
 int seed=4;
 if(int x=seed;int y=x-1) total+=y; else total=100;
 while(int value=seed){total+=value;seed-=1;}
 if(int x=1;int& r=x){r=2;total+=x;}
 return total;
}
static_assert(gate()==21);
