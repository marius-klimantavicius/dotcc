// Exact byte data rendered as C# UTF-8 literals or numeric span literals.
// Never decode with replacement: binary data must keep every original byte.
bool source_utf8_scalar(const string &bytes, size_t &position, unsigned &scalar) {
  unsigned first=(unsigned char)bytes[position++];
  if(first<0x80){scalar=first;return true;}
  unsigned remaining,minimum;
  if(first>=0xc2 && first<=0xdf){remaining=1;minimum=0x80;scalar=first&0x1f;}
  else if(first>=0xe0 && first<=0xef){remaining=2;minimum=0x800;scalar=first&0x0f;}
  else if(first>=0xf0 && first<=0xf4){remaining=3;minimum=0x10000;scalar=first&7;}
  else return false;
  if(remaining>bytes.size()-position)return false;
  while(remaining--) {
    unsigned next=(unsigned char)bytes[position++];
    if((next&0xc0)!=0x80)return false;
    scalar=(scalar<<6)|(next&0x3f);
  }
  return scalar>=minimum && scalar<=0x10ffff && !(scalar>=0xd800 && scalar<=0xdfff);
}
bool source_valid_utf8(const string &bytes) {
  size_t position=0;unsigned scalar;
  while(position<bytes.size())if(!source_utf8_scalar(bytes,position,scalar))return false;
  return true;
}
string source_utf8_text(const string &bytes,size_t scalar_limit,bool comment) {
  string result;size_t position=0,count=0;
  while(position<bytes.size() && count<scalar_limit) {
    size_t begin=position;unsigned scalar;
    if(!source_utf8_scalar(bytes,position,scalar))return "";
    ++count;
    switch(scalar) {
      case 0:result+="\\0";break;
      case '\n':result+="\\n";break;
      case '\r':result+="\\r";break;
      case '\t':result+="\\t";break;
      case '\b':result+="\\b";break;
      case '\f':result+="\\f";break;
      case '\\':result+="\\\\";break;
      case '"':result+="\\\"";break;
      default:
        // Keep physical lines and comment boundaries intact. Escape C# newline
        // characters and invisible direction controls even in valid UTF-8.
        if(scalar<0x20 || (scalar>=0x7f && scalar<=0x9f) ||
           scalar==0x2028 || scalar==0x2029 || (scalar>=0x202a && scalar<=0x202e) ||
           (scalar>=0x2066 && scalar<=0x2069) ||
           (comment && scalar=='/' && begin && bytes[begin-1]=='*')) {
          char escape[16];snprintf(escape,sizeof(escape),"\\u%04x",scalar);result+=escape;
        } else result.append(bytes,begin,position-begin);
    }
  }
  if(position<bytes.size())result+="...";
  return result;
}
string source_literal_preview(const string &bytes) {
  if(!source_valid_utf8(bytes))return "";
  // The storage terminator is not useful in a navigation comment; embedded
  // NUL bytes remain visible. Preview text never participates in evaluation.
  string text=bytes;
  if(!text.empty() && text.back()==0)text.pop_back();
  return " /* \""+source_utf8_text(text,80,true)+"\" */";
}
string source_byte_copy(const string &address,const string &bytes,bool allow_utf8) {
  string result;
  // Bound individual literals and method statements while avoiding a managed
  // heap allocation for every initialization. Constant spans use static data.
  const size_t chunk_size=16384;
  bool utf8=allow_utf8 && source_valid_utf8(bytes);
  for(size_t offset=0;offset<bytes.size();) {
    size_t end=std::min(bytes.size(),offset+chunk_size);
    if(utf8 && end<bytes.size())
      while(end>offset && ((unsigned char)bytes[end]&0xc0)==0x80)--end;
    string chunk=bytes.substr(offset,end-offset);
    string id="__edg_initializer"+std::to_string(++serial);
    result+="global::System.ReadOnlySpan<byte> "+id+" = ";
    if(utf8)result+="\""+source_utf8_text(chunk,chunk.size(),false)+"\"u8";
    else {
      result+="[";
      for(size_t i=0;i<chunk.size();++i) {
        if(i)result+=", ";
        result+=std::to_string((unsigned char)chunk[i]);
      }
      result+="]";
    }
    result+=";\n"+id+".CopyTo(new global::System.Span<byte>((byte*)("+address+")"+
        (offset?" + "+std::to_string(offset):"")+", "+id+".Length));\n";
    offset=end;
  }
  return result;
}
