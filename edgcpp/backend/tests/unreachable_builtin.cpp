static int reached;
static int choose(int value) {
 if(value==3)return 19;
 __builtin_unreachable();
}
static void branch(bool enter) {if(enter)__builtin_unreachable();++reached;}
int main() {branch(false);return choose(3)==19 && reached==1?0:1;}
