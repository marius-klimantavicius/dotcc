struct Cell { int value; unsigned char marker; };
struct Big { Cell cells[20000]; long guard; };
Big global_big;
thread_local Big thread_big;
int global_repeat[100000]={};
int (*function_cells[24000])(int);
int* pointer_cells[24000];
int increase(int n) { return n+1; }
Big transform(Big value) {
  value.cells[19999].value+=2;value.cells[0].marker+=3;value.guard+=5;
  return value;
}
int main() {
  if(sizeof(Big)!=160008 || sizeof(global_repeat)!=400000)return 1;
  Big local={};local.cells[0].marker=7;local.cells[19999].value=11;local.guard=13;
  global_big=transform(local);
  if(global_big.cells[0].marker!=10 || global_big.cells[19999].value!=13 || global_big.guard!=18)return 2;
  if(local.cells[0].marker!=7 || local.cells[19999].value!=11 || local.guard!=13)return 3;
  thread_big=global_big;global_big.cells[19999].value=99;
  if(thread_big.cells[19999].value!=13)return 4;
  int value=17;pointer_cells[23999]=&value;function_cells[23999]=increase;
  if(function_cells[23999](*pointer_cells[23999])!=18)return 5;
  global_repeat[99999]=91;
  if(global_repeat[0] || global_repeat[99998] || global_repeat[99999]!=91)return 6;
  return 0;
}
