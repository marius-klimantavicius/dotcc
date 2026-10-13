/* Exact retained physical files, include occurrences, remapped descriptors and positions. */
static std::map<std::string,Rid> roslyn_physical_files;
static std::map<a_source_file_ptr,Rid> roslyn_file_occurrences,roslyn_line_directives;
struct RoslynSourceMapping { a_seq_number first,last; Rid id; };
static std::vector<RoslynSourceMapping> roslyn_source_mappings;
static bool roslyn_has_position(const a_source_position &p) {
 if(p.seq||p.column)return true;
#if FULLY_RESOLVED_MACRO_POSITIONS
 if(p.orig_seq||p.orig_column)return true;
#endif
#if RECORD_MACRO_INVOCATIONS
 if(p.macro_context)return true;
#endif
 return false;
}
static a_macro_invocation_record_index roslyn_macro_context(const a_source_position &p) {
#if RECORD_MACRO_INVOCATIONS
 return p.macro_context;
#else
 return 0;
#endif
}
static const char *roslyn_position_kind(const a_source_position &p) {
 if(roslyn_macro_context(p))return "macro";
 if(p.seq)return "ordinary";
 switch(p.column){case SP_COL_CMD_LINE:return "command_line";case SP_PREINCLUDE:return "preinclude";case SP_COL_PREDEFINED_MACRO:return "predefined";default:return "unknown";}
}
static Rid roslyn_source_file(a_source_file_ptr p,Rid,int) {
 if(!p)return 0;
 if(!p->full_name){a_line_number line=0;a_boolean end=FALSE;p=source_file_for_seq(p->first_seq_number,&line,&end,TRUE);if(!p||!p->full_name)return 0;}
 auto known=roslyn_physical_files.find(p->full_name);if(known!=roslyn_physical_files.end())return known->second;
 Rid id=DB.fresh("source_file");roslyn_physical_files.emplace(p->full_name,id);
 DB.put("source_file",{{"id",id},{"path_id",DB.atom(p->full_name)},{"origin_kind_id",K("source_origin_kind","ordinary")}});return id;
}
static Rid roslyn_source_descriptor(a_source_file_ptr p,Rid parent_include,Rid parent_directive,Rid parent_descriptor,int ordinal) {
 Rid descriptor=0,current_include=parent_include,current_directive=parent_directive;
 if(p->full_name){bool fresh;descriptor=DB.claim(p,"include_occurrence",fresh);if(!fresh)return descriptor;current_include=descriptor;current_directive=0;roslyn_file_occurrences[p]=descriptor;int implicit=0;
#if INSTANTIATION_BY_IMPLICIT_INCLUSION
 implicit=p->is_implicit_include;
#endif
 DB.put("include_occurrence",{{"id",descriptor},{"file_id",roslyn_source_file(p)},{"parent_id",R(parent_include)},{"ordinal",ordinal},{"display_name_id",R(DB.atom(p->file_name))},{"name_as_written_id",R(DB.atom(p->name_as_written))},{"first_sequence",Q(p->first_seq_number)},{"last_sequence",Q(p->last_seq_number)},{"first_line",Q(p->first_line_number)},{"module_id",R(roslyn_module(p->assoc_module))},{"is_system",static_cast<int>(p->from_system_include_dir)},{"uses_angle_brackets",static_cast<int>(p->included_by_system_include)},{"is_include_file",static_cast<int>(p->is_include_file)},{"is_preinclude",static_cast<int>(p->included_by_preinclude)},{"is_macros_only",static_cast<int>(p->preinclude_macros_only)},{"is_top_level",static_cast<int>(p->top_level_file)},{"is_top_level_from_pch",static_cast<int>(p->top_level_file_from_pch)},{"is_implicit_include",implicit}});
 }else{bool fresh;descriptor=DB.claim(p,"line_directive",fresh);if(!fresh)return descriptor;current_directive=descriptor;roslyn_line_directives[p]=descriptor;
 DB.put("line_directive",{{"id",descriptor},{"presumed_name_id",DB.atom(p->file_name)},{"first_sequence",Q(p->first_seq_number)},{"last_sequence",Q(p->last_seq_number)},{"first_line",Q(p->first_line_number)},{"include_id",R(parent_include)},{"parent_id",R(parent_directive)}});
 }
 DB.put("source_descriptor_child",{{"owner_id",parent_descriptor},{"ordinal",ordinal},{"child_id",descriptor}});
 int child_ordinal=0;for(a_source_file_ptr child=p->first_child_file;child;child=child->next)roslyn_source_descriptor(child,current_include,current_directive,descriptor,child_ordinal++);
 return descriptor;
}
static void roslyn_source_information() {
 roslyn_physical_files.clear();roslyn_file_occurrences.clear();roslyn_line_directives.clear();roslyn_source_mappings.clear();
 int ordinal=0;for(a_source_file_ptr p=il_header.primary_source_file;p;p=p->next)roslyn_source_descriptor(p,0,0,roslyn_unit,ordinal++);
 for(a_seq_number_lookup_entry_ptr p=il_header.seq_number_lookup_entries;p;p=p->next){
  if(!p->source_file||!p->first||p->first>p->last)continue;
  a_line_number physical_line=0,last_line=0;a_boolean end=FALSE,last_end=FALSE;
  a_source_file_ptr physical=source_file_for_seq(p->first,&physical_line,&end,TRUE);
  source_file_for_seq(p->last,&last_line,&last_end,TRUE);
  if(!physical){DB.fail("Retained source mapping has no physical source file");continue;}
  Rid include=0,directive=0;auto inc=roslyn_file_occurrences.find(physical);if(inc!=roslyn_file_occurrences.end())include=inc->second;auto remap=roslyn_line_directives.find(p->source_file);if(remap!=roslyn_line_directives.end())directive=remap->second;
  bool fresh;Rid id=DB.claim(p,"source_mapping",fresh);if(!fresh)continue;
  DB.put("source_mapping_segment",{{"id",id},{"first_sequence",Q(p->first)},{"last_sequence",Q(p->last)},{"file_id",roslyn_source_file(physical)},{"include_id",R(include)},{"line_directive_id",R(directive)},{"logical_line",Q(p->line_number)},{"physical_line",Q(physical_line)},{"presumed_name_id",R(DB.atom(p->source_file->file_name))},{"ends_at_end_of_source",static_cast<int>(last_end)}});
  roslyn_source_mappings.push_back({p->first,p->last,id});
 }
 std::sort(roslyn_source_mappings.begin(),roslyn_source_mappings.end(),[](const RoslynSourceMapping &a,const RoslynSourceMapping &b){return a.first<b.first;});
}
static Rid roslyn_source_mapping(a_seq_number sequence) {
 auto upper=std::upper_bound(roslyn_source_mappings.begin(),roslyn_source_mappings.end(),sequence,[](a_seq_number n,const RoslynSourceMapping &p){return n<p.first;});
 if(upper==roslyn_source_mappings.begin())return 0;--upper;return sequence<=upper->last?upper->id:0;
}
static Rid roslyn_position(const a_source_position &p) {
 if(!roslyn_has_position(p))return 0;
 a_macro_invocation_record_index context=roslyn_macro_context(p);a_seq_number original_sequence=p.seq;a_column_number original_column=p.column;RoslynSqlValue original_sequence_value,original_column_value;
#if FULLY_RESOLVED_MACRO_POSITIONS
 original_sequence=p.orig_seq;original_column=p.orig_column;original_sequence_value=Q(p.orig_seq);original_column_value=static_cast<int>(p.orig_column);
#endif
 std::string key=Q(p.seq)+":"+Q(p.column)+":"+Q(original_sequence)+":"+Q(original_column)+":"+Q(context);auto found=roslyn_positions.find(key);if(found!=roslyn_positions.end())return found->second;
 Rid id=DB.fresh("source_position");roslyn_positions[key]=id;
 Rid macro=0;auto mf=roslyn_macro_invocations.find(context);if(mf!=roslyn_macro_invocations.end())macro=mf->second;
 a_line_number logical_line=0,physical_line=0;a_boolean end=FALSE,physical_end=FALSE;a_source_file_ptr logical=p.seq?source_file_for_seq(p.seq,&logical_line,&end,FALSE):nullptr;a_source_file_ptr physical=p.seq?source_file_for_seq(p.seq,&physical_line,&physical_end,TRUE):nullptr;
 Rid include=0;auto inc=roslyn_file_occurrences.find(physical);if(inc!=roslyn_file_occurrences.end())include=inc->second;
 Rid spelling=0;
#if FULLY_RESOLVED_MACRO_POSITIONS
 if(p.seq!=p.orig_seq||p.column!=p.orig_column){a_source_position original={};original.seq=original.orig_seq=p.orig_seq;original.column=original.orig_column=p.orig_column;spelling=roslyn_position(original);}
#endif
 DB.put("source_position",{{"id",id},{"sequence",Q(p.seq)},{"column",static_cast<int>(p.column)},{"original_sequence",original_sequence_value},{"original_column",original_column_value},{"macro_invocation_id",R(macro)},{"file_id",R(roslyn_source_file(physical))},{"presumed_name_id",R(logical?DB.atom(logical->file_name):0)},{"logical_line",logical?RoslynSqlValue(Q(logical_line)):RoslynSqlValue()},{"physical_line",physical?RoslynSqlValue(Q(physical_line)):RoslynSqlValue()},{"include_id",R(include)},{"mapping_segment_id",R(roslyn_source_mapping(p.seq))},{"spelling_position_id",R(spelling)},{"at_end_of_source",static_cast<int>(end||physical_end)},{"origin_kind_id",K("source_origin_kind",roslyn_position_kind(p))}});return id;
}
static Rid roslyn_range(const a_source_range &range) {
 Rid first=roslyn_position(range.start),last=roslyn_position(range.end);if(!first||!last)return 0;
 auto key=std::make_pair(first,last);auto known=roslyn_ranges.find(key);if(known!=roslyn_ranges.end())return known->second;
 Rid id=DB.fresh("source_range");roslyn_ranges[key]=id;DB.put("source_range",{{"id",id},{"start_id",first},{"end_id",last},{"endpoint_kind_id",K("range_endpoint","inclusive")}});return id;
}
static Rid roslyn_origin(const a_source_position &p,const a_source_position *end=nullptr,const a_source_position *start=nullptr) {
 Rid pos=roslyn_position(p);if(!pos&&(!start||!roslyn_has_position(*start)))return 0;Rid range=0;
 if(end&&roslyn_has_position(*end)){Rid first=start?roslyn_position(*start):pos,last=roslyn_position(*end);if(first){auto key=std::make_pair(first,last);auto known=roslyn_ranges.find(key);if(known!=roslyn_ranges.end())range=known->second;else{range=DB.fresh("source_range");roslyn_ranges[key]=range;DB.put("source_range",{{"id",range},{"start_id",first},{"end_id",last},{"endpoint_kind_id",K("range_endpoint","inclusive")}});}}}
 auto key=std::make_pair(pos,range);auto known=roslyn_origins.find(key);if(known!=roslyn_origins.end())return known->second;
 Rid n=DB.fresh("source_origin");roslyn_origins[key]=n;DB.put("source_origin",{{"id",n},{"position_id",R(pos)},{"range_id",R(range)},{"kind_id",K("source_origin_kind",roslyn_position_kind(pos?p:*start))}});return n;
}
