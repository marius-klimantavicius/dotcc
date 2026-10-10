extern thread_local int value;
int main() { if(value!=7)return 1; ++value; return value!=8; }
thread_local int value=7;
