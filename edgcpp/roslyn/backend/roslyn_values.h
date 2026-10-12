/* Exact typed value content. Interning keys are transient, never persisted. */
struct RoslynContent {
 typedef std::pair<const char*,RoslynSqlValue> Field;
 struct Row {const char *table,*owner_column;std::vector<Field> fields;};
 int kind;std::vector<Row> rows;
 explicit RoslynContent(a_constant_repr_kind repr):kind(roslyn_kind_constant(repr)){}
 void add(const char *table,std::initializer_list<Field> fields,const char *owner="id"){rows.push_back({table,owner,std::vector<Field>(fields)});}
 Rid intern() {
  std::string key="typed:"+Q(kind);
  auto append=[&](const std::string &part){key+=':';key+=Q(part.size());key+=':';key+=part;};
  for(const auto &row:rows){append(row.table);append(row.owner_column);for(const auto &f:row.fields){append(f.first);append(Q(f.second.tag));if(f.second.tag==1)append(Q(f.second.number));else if(f.second.tag==2)append(f.second.text);}}
  auto found=roslyn_values.find(key);if(found!=roslyn_values.end())return found->second;
  Rid id=DB.fresh("constant_value");roslyn_values[key]=id;DB.put("constant_value",{{"id",id},{"kind_id",kind}});
  for(auto &row:rows){row.fields.insert(row.fields.begin(),{row.owner_column,id});DB.put_fields(row.table,row.fields);}
  return id;
 }
};
static void roslyn_float_content(RoslynContent &content,a_float_kind kind,const an_internal_float_value &value) {
 auto f=roslyn_float_parts(kind,value);
 if(f.classification=="unsupported"){DB.fail("Unsupported active floating representation");return;}
 content.add("floating_value",{{"class_id",K("float_class",f.classification.c_str())},
  {"negative",static_cast<int>(f.negative)},{"radix",2},{"significand_id",DB.atom(f.significand)},
  {"exponent",static_cast<sqlite3_int64>(f.exponent)},{"nan_payload_id",R(f.payload.empty()?0:DB.atom(f.payload))},
  {"nan_quiet",static_cast<int>(f.quiet)},{"representation_kind_id",K("float_representation",f.format.c_str())}});
}
static Rid roslyn_float_value(a_float_kind kind,const an_internal_float_value &value) {
 RoslynContent content(ck_float);roslyn_float_content(content,kind,value);return content.intern();
}
static Rid roslyn_value_of(a_constant_ptr p) {
 if(!p)return 0;roslyn_constant(p);auto found=roslyn_constant_values.find(p);
 if(found==roslyn_constant_values.end()){DB.fail("Cyclic inline constant content");return 0;}return found->second;
}
static void roslyn_constant_component(Rid owner,const char *role,int ordinal,a_constant_ptr p) {
 if(p)DB.put("constant_component",{{"owner_id",owner},{"role_id",K("constant_component_role",role)},
  {"ordinal",ordinal},{"constant_id",roslyn_constant(p)}});
}
