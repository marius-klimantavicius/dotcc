#include <string>
#include <vector>
#include "../dot_gen_ir.h"
int main() {
 dot_ir::Graph graph;
 auto integer=graph.reserve();graph.types[integer].kind="signed-integer";graph.types[integer].size=4;graph.types[integer].alignment=4;
 auto record=graph.reserve();graph.types[record].kind="record";graph.types[record].size=16;graph.types[record].alignment=8;
 auto pointer=graph.reserve();graph.types[pointer].kind="pointer";graph.types[pointer].size=8;graph.types[pointer].alignment=8;graph.types[pointer].element=record;
 graph.types[record].fields.push_back({"next",pointer,0,0,0,false});
 graph.types[record].fields.push_back({"value",integer,8,0,0,false});
 if(!graph.verify().empty())return 1;
 auto invalid=graph;invalid.types[record].fields[0].type=record;if(invalid.verify().empty())return 2;
 invalid=graph;invalid.types[record].fields[1].offset=15;if(invalid.verify().empty())return 3;
 invalid=graph;invalid.types[pointer].element=999;if(invalid.verify().empty())return 4;
 invalid=graph;invalid.types[record].fields.push_back({"badbits",integer,16,1,1,true});if(invalid.verify().empty())return 5;
 invalid=graph;invalid.types[record].alignment=3;if(invalid.verify().empty())return 6;
 auto array=graph.reserve();graph.types[array].kind="array";graph.types[array].element=record;graph.types[array].size=48;graph.types[array].count=3;graph.types[array].complete=true;
 if(!graph.verify().empty())return 7;
 graph.types[array].count=4;if(graph.verify().empty())return 8;
 return 0;
}
