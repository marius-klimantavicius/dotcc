struct Opaque;
Opaque* opaque_identity(Opaque* value) {return value;}
struct Node { int value;Node* next;int(*callback)(int);unsigned flags:3;signed delta:5; };
union Choice {unsigned long word;Node* node;};
struct Table {Node entries[3];Choice choice;};
static const volatile int* qualified;
static int increment(int value){return value+1;}
int main(){
 Table table{};table.entries[0].value=31;table.entries[0].next=&table.entries[1];
 table.entries[0].callback=increment;table.entries[0].flags=5;table.entries[0].delta=-7;
 table.choice.node=&table.entries[0];qualified=&table.entries[0].value;
 if(*qualified!=31 || table.choice.node->callback(41)!=42 || table.entries[0].delta!=-7)return 1;
 if(opaque_identity((Opaque*)&table)!= (Opaque*)&table || table.entries[0].next!=&table.entries[1])return 2;
 return 0;
}
