/* Contextual references to retained function-local expressions and VLA descriptors. */
struct RoslynLocalExpression { a_scope_ptr scope; a_local_expr_node_ref_ptr entry; };
struct RoslynVlaContext { a_scope_ptr scope; a_vla_dimension_ptr dimension; };
static std::map<std::pair<const void*,int>,std::vector<RoslynLocalExpression>> roslyn_local_expressions;
static std::map<a_type_ptr,std::vector<RoslynVlaContext>> roslyn_vla_contexts;
static std::set<a_scope_ptr> roslyn_collected_scopes;
/* A descriptor stored outside a function has no chosen function context.
   Its scalar convenience link exists only when all retained occurrences
   identify the same expression. Scope-specific links remain in the graph. */
static an_expr_node_ptr roslyn_unique_local_expression(const void *referrer,a_local_expr_node_ref_kind kind) {
 auto found=roslyn_local_expressions.find({referrer,static_cast<int>(kind)});if(found==roslyn_local_expressions.end())return nullptr;
 an_expr_node_ptr result=nullptr;for(const auto &reference:found->second){if(result&&result!=reference.entry->expr)return nullptr;result=reference.entry->expr;}return result;
}
static an_expr_node_ptr roslyn_local_expression(const void *referrer,a_local_expr_node_ref_kind kind) {
 auto found=roslyn_local_expressions.find({referrer,static_cast<int>(kind)});if(found==roslyn_local_expressions.end())return nullptr;
 an_expr_node_ptr result=nullptr;for(const auto &reference:found->second){if(roslyn_current_routine&&roslyn_scope_routine(reference.scope)!=roslyn_current_routine)continue;if(result&&result!=reference.entry->expr)return nullptr;result=reference.entry->expr;}return result;
}
static a_vla_dimension_ptr roslyn_type_vla_dimension(a_type_ptr type) {
 auto found=roslyn_vla_contexts.find(type);if(found==roslyn_vla_contexts.end())return nullptr;
 a_vla_dimension_ptr result=nullptr;for(const auto &context:found->second){if(roslyn_current_routine&&roslyn_scope_routine(context.scope)!=roslyn_current_routine)continue;if(result&&result!=context.dimension)return nullptr;result=context.dimension;}return result;
}
static void roslyn_collect_local_initializers(a_scope_ptr scope) {
 if(!scope||!roslyn_collected_scopes.insert(scope).second)return;
 for(a_local_scope_ref_ptr p=scope->scope_refs;p;p=p->next)roslyn_scope_parents[p->referrer.ptr]=p->scope;
 for(a_local_static_variable_init_ptr p=scope->local_static_variable_inits;p;p=p->next)if(p->init_kind==initk_static)roslyn_local_constants[p->variable]=p->initializer.constant;
 for(a_local_expr_node_ref_ptr p=scope->expr_node_refs;p;p=p->next)roslyn_local_expressions[{p->referrer.ptr,static_cast<int>(p->kind)}].push_back({scope,p});
 for(a_vla_dimension_ptr p=scope->vla_dimensions;p;p=p->next)roslyn_vla_contexts[p->type].push_back({scope,p});
 for(a_scope_ptr p=scope->scopes;p;p=p->next)roslyn_collect_local_initializers(p);
}
static Rid roslyn_scoped_expression(a_scoped_expression_ptr p) {
 bool fresh;Rid id=DB.claim(p,"scoped_expression",fresh);if(!fresh)return id;
 an_expr_node_ptr expression=p->expr?p->expr:roslyn_local_expression(p,lerk_scoped_expr);
 DB.put("scoped_expression",{{"id",id},{"expression_id",roslyn_expr(expression,7002)},{"source_origin_id",R(roslyn_origin(p->source_corresp.decl_position))}});return id;
}
static void roslyn_scope_expression_references(a_scope_ptr scope,Rid owner) {
 int ordinal=0;for(a_local_expr_node_ref_ptr p=scope->expr_node_refs;p;p=p->next){
  Rid referrer=roslyn_entity(p->referrer),expression=roslyn_expr(p->expr,7002);
  if(p->kind==lerk_none||!referrer||!expression){DB.fail("Invalid retained local expression reference");continue;}
  DB.put("local_expression_reference",{{"owner_id",owner},{"ordinal",ordinal++},{"kind_id",roslyn_kind_edg_local_expression_kind(p->kind)},{"referrer_id",referrer},{"expression_id",expression}});
 }
}
