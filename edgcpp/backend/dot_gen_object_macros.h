#ifndef DOT_GEN_OBJECT_MACROS_H
#define DOT_GEN_OBJECT_MACROS_H
// Owned observations of accepted preprocessor state changes. This deliberately
// does not model expansion occurrences or preserve original directive trivia.
#include <cstdlib>
#include <map>
#include <string>
#include <vector>
#include "fe_common.h"
#include "il.h"
BEGIN_EDG_NAMESPACE
namespace dot_object_macros {
inline bool enabled() {
  static const bool value=[](){const char *v=std::getenv("EDG_DOT_OBJECT");return v && std::string(v)=="1";}();
  return value;
}
struct Definition {
  std::string name,text,origin;
  std::vector<std::string> parameters;
  bool object_like=false,variadic=false,dynamic=false;
  a_source_position position={};
};
struct Event {
  std::string kind,name,origin;
  unsigned definition=0;
  a_source_position position={};
};
struct State {
  std::vector<Definition> definitions;
  std::vector<Event> events;
  std::string unsupported;
  std::map<std::string,unsigned> installed;
  std::map<std::string,std::vector<unsigned>> saved;
};
inline State &state(){static State value;return value;}
inline void reject_feature(const char *name) {
  if(enabled() && state().unsupported.empty())state().unsupported=name;
}
inline bool valid_text(const std::string &text) {
  // Wire strings are UTF-8, never a lossy reinterpretation of source bytes.
  for(size_t i=0;i<text.size();) {
    unsigned c=(unsigned char)text[i++];if(!c)return false;
    if(c<128)continue;
    unsigned count=c>=0xc2&&c<=0xdf?1:c>=0xe0&&c<=0xef?2:c>=0xf0&&c<=0xf4?3:0;
    if(!count || i+count>text.size())return false;
    unsigned value=c&((1u<<(6-count))-1);
    for(unsigned n=0;n<count;++n){unsigned next=(unsigned char)text[i++];if((next&0xc0)!=0x80)return false;value=(value<<6)|(next&0x3f);}
    if(value<(count==1?0x80u:count==2?0x800u:0x10000u) || value>0x10ffff || (value>=0xd800&&value<=0xdfff))return false;
  }
  return true;
}
inline void define(const Definition &value) {
  if(!enabled())return;
  if(!valid_text(value.name)||!valid_text(value.text))reject_feature("non-UTF8 or NUL macro text");
  for(const auto &parameter:value.parameters)if(!valid_text(parameter))reject_feature("non-UTF8 or NUL macro parameter");
  auto &s=state();s.definitions.push_back(value);
  Event event;event.kind="define";event.name=value.name;event.origin=value.origin;
  event.position=value.position;event.definition=(unsigned)s.definitions.size();s.events.push_back(event);
  s.installed[value.name]=event.definition;
}
inline void undef(const char *name,size_t length,const a_source_position &position,const char *origin) {
  if(!enabled())return;
  Event event;event.kind="undef";event.name.assign(name,length);event.origin=origin;event.position=position;
  if(!valid_text(event.name))reject_feature("non-UTF8 or NUL macro name");
  state().events.push_back(event);
  state().installed.erase(event.name);
}
inline void push(const char *name,const a_source_position &position) {
  if(!enabled())return;
  auto &s=state();unsigned definition=s.installed[name];
  s.saved[name].push_back(definition);
  Event event;event.kind="push";event.name=name;event.origin="source";
  event.position=position;event.definition=definition;s.events.push_back(event);
}
inline void pop(const char *name,const a_source_position &position) {
  if(!enabled())return;
  auto &s=state();auto &saved=s.saved[name];
  if(saved.empty()){reject_feature("macro stack observation mismatch");return;}
  unsigned definition=saved.back();saved.pop_back();s.installed[name]=definition;
  Event event;event.kind="pop";event.name=name;event.origin="source";
  event.position=position;event.definition=definition;s.events.push_back(event);
}
}
END_EDG_NAMESPACE
#endif
