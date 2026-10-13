-- A small valid normalized object. IDs are local fixtures, never public navigation.
INSERT INTO text_atom VALUES
 (1,'reader fixture'),(2,'89e67e07c7f6d0fc38622f9778d8e79f40deaa0f'),(3,'7.0'),
 (4,'C++20'),(5,'compute'),(6,'x'),(7,'helper'),(8,'int128'),(9,'Widget'),(10,'value'),
 (11,'340282366920938463463374607431768211455'),(12,'fixture.cpp'),(13,'SCALE'),
 (14,'#define SCALE(x) ((x)*2)'),(15,'( 4 )'),(16,'T'),(17,'normalized_semantic_graph_v1'),
 (18,'exact_target_values_v1'),(19,'edg_partial_eh_v1'),(20,'1'),(21,'0');
INSERT INTO entity(id,kind_id) SELECT value, k.id FROM
 (SELECT 1 value,'translation_unit' name UNION ALL SELECT 2,'scope' UNION ALL SELECT 3,'type'
  UNION ALL SELECT 4,'type' UNION ALL SELECT 5,'name' UNION ALL SELECT 6,'routine'
  UNION ALL SELECT 7,'variable' UNION ALL SELECT 8,'name' UNION ALL SELECT 9,'scope'
  UNION ALL SELECT 10,'statement' UNION ALL SELECT 11,'expression' UNION ALL SELECT 12,'expression'
  UNION ALL SELECT 13,'expression' UNION ALL SELECT 14,'statement' UNION ALL SELECT 15,'statement'
  UNION ALL SELECT 20,'routine' UNION ALL SELECT 21,'name' UNION ALL SELECT 30,'constant'
  UNION ALL SELECT 31,'constant_value' UNION ALL SELECT 40,'type' UNION ALL SELECT 41,'name'
  UNION ALL SELECT 42,'field' UNION ALL SELECT 43,'name' UNION ALL SELECT 44,'record_layout'
  UNION ALL SELECT 50,'function_eh' UNION ALL SELECT 51,'cleanup_region' UNION ALL SELECT 52,'cleanup_action'
  UNION ALL SELECT 53,'cleanup_storage' UNION ALL SELECT 54,'cleanup_array' UNION ALL SELECT 55,'cleanup_condition'
  UNION ALL SELECT 56,'cleanup_abi' UNION ALL SELECT 60,'expression' UNION ALL SELECT 61,'statement'
  UNION ALL SELECT 70,'macro_definition' UNION ALL SELECT 71,'macro_invocation' UNION ALL SELECT 72,'source_position'
  UNION ALL SELECT 73,'source_origin' UNION ALL SELECT 74,'source_file' UNION ALL SELECT 80,'lifetime'
  UNION ALL SELECT 81,'block_descriptor' UNION ALL SELECT 82,'constant' UNION ALL SELECT 83,'constant_value' UNION ALL SELECT 84,'type'
  UNION ALL SELECT 90,'template' UNION ALL SELECT 91,'template_argument' UNION ALL SELECT 92,'template_instance'
  UNION ALL SELECT 93,'template_parameter' UNION ALL SELECT 94,'name' UNION ALL SELECT 95,'template_symbol' UNION ALL SELECT 96,'include_occurrence'
  UNION ALL SELECT 100,'constant_value' UNION ALL SELECT 101,'constant' UNION ALL SELECT 102,'type'
  UNION ALL SELECT 105,'type' UNION ALL SELECT 106,'type' UNION ALL SELECT 103,'constant_value' UNION ALL SELECT 104,'constant'
  UNION ALL SELECT 150,'expression' UNION ALL SELECT 170,'parameter_type') a JOIN kind k ON k.family='entity' AND k.name=a.name;
INSERT INTO target_profile(id,abi_id,addressable_unit_bits,little_endian,pointer_bits,pointer_alignment)
 VALUES(1,(SELECT id FROM kind WHERE family='abi' AND name='itanium'),8,1,64,'8');
INSERT INTO scope(id,kind_id,parent_id,associated_entity_id)
 VALUES(2,(SELECT id FROM kind WHERE family='scope_kind' AND name='file'),NULL,1),
 (9,(SELECT id FROM kind WHERE family='scope_kind' AND name='function'),2,6);
INSERT INTO translation_unit(id,global_scope_id,primary_input_id,language_mode_id) VALUES(1,2,74,4);
INSERT INTO object_header VALUES(1,1,0,'fixture-artifact',1,2,3,
 (SELECT id FROM kind WHERE family='capture_profile' AND name='native_lowered'),1,74,1,1);
INSERT INTO build_capability(name_id,state_id,value_id,required,enabled)
 SELECT name_id,(SELECT id FROM kind WHERE family='availability' AND name='available'),20,1,1
 FROM (SELECT 17 name_id UNION ALL SELECT 18 UNION ALL SELECT 19);
INSERT INTO name(id,spelling_id,category_id) SELECT id,spelling,
 (SELECT id FROM kind WHERE family='name_category' AND name='named')
 FROM (SELECT 5 id,5 spelling UNION ALL SELECT 8,6 UNION ALL SELECT 21,7 UNION ALL SELECT 41,9 UNION ALL SELECT 43,10 UNION ALL SELECT 94,16);
INSERT INTO type(id,kind_id,name_id,size,alignment,incomplete) VALUES
 (3,(SELECT id FROM kind WHERE family='type' AND name='tk_integer'),NULL,'16','16',0),
 (4,(SELECT id FROM kind WHERE family='type' AND name='tk_routine'),NULL,'0','1',0),
 (40,(SELECT id FROM kind WHERE family='type' AND name='tk_struct'),41,'16','16',0),
 (102,(SELECT id FROM kind WHERE family='type' AND name='tk_float'),NULL,'8','8',0),
 (105,(SELECT id FROM kind WHERE family='type' AND name='tk_array'),NULL,'6','2',0),
 (106,(SELECT id FROM kind WHERE family='type' AND name='tk_integer'),NULL,'2','2',0);
INSERT INTO arithmetic_type(id,arithmetic_kind_id,bits,signed,precision,radix) VALUES
 (3,(SELECT id FROM kind WHERE family='arithmetic_kind' AND name='unsigned_integer'),128,0,128,2),
 (102,(SELECT id FROM kind WHERE family='arithmetic_kind' AND name='floating'),64,1,53,2),
 (106,(SELECT id FROM kind WHERE family='arithmetic_kind' AND name='unsigned_integer'),16,0,16,2);
INSERT INTO array_type(id,element_type_id,bound_kind_id,bound) VALUES(105,106,(SELECT id FROM kind WHERE family='bound_kind' AND name='constant'),'3');
INSERT INTO routine_type(id,result_type_id) VALUES(4,3);
INSERT INTO parameter_type(id,type_id,parameter_number) VALUES(170,3,1);
INSERT INTO routine_type_parameter(owner_id,ordinal,parameter_type_id,role_id)
 VALUES(4,0,170,(SELECT id FROM kind WHERE family='role' AND name='ordinary_parameter'));
INSERT INTO symbol(id,kind_id,name_id,containing_entity_id,type_id) SELECT a.id,k.id,name_id,owner_id,type_id
 FROM (SELECT 6 id,'routine' n,5 name_id,1 owner_id,4 type_id UNION ALL SELECT 20,'routine',21,1,4
 UNION ALL SELECT 7,'variable',8,6,3 UNION ALL SELECT 42,'field',43,40,3) a JOIN kind k ON k.family='entity' AND k.name=a.n;
INSERT INTO routine_symbol(id,routine_type_id,body_root_id,body_scope_id,body_state_id) VALUES
 (6,4,10,9,(SELECT id FROM kind WHERE family='body_state' AND name='definition')),
 (20,4,NULL,NULL,(SELECT id FROM kind WHERE family='body_state' AND name='declaration_only'));
INSERT INTO variable_symbol(id,type_id,is_parameter,initialization_kind_id) VALUES(7,3,1,(SELECT id FROM kind WHERE family='initializer' AND name='initk_none'));
INSERT INTO routine_parameter(owner_id,ordinal,parameter_id,role_id,is_source_parameter)
 VALUES(6,0,7,(SELECT id FROM kind WHERE family='role' AND name='ordinary_parameter'),1);
INSERT INTO scope_member VALUES(2,0,6),(2,1,20),(2,2,40),(9,0,7);
INSERT INTO semantic_node(id,kind_id,representation_role_id,source_origin_id)
 SELECT a.id,k.id,(SELECT id FROM kind WHERE family='representation_role' AND name=a.role),
 CASE WHEN a.id=11 THEN 73 ELSE NULL END FROM
 (SELECT 10 id,'statement' f,'stmk_block' n,'execution' role UNION ALL SELECT 11,'expression','enk_operation','execution'
 UNION ALL SELECT 12,'expression','enk_routine','execution' UNION ALL SELECT 13,'expression','enk_constant','execution'
 UNION ALL SELECT 14,'statement','stmk_expr','execution' UNION ALL SELECT 15,'statement','stmk_return','execution'
 UNION ALL SELECT 60,'expression','enk_lowered_eh_construct','execution' UNION ALL SELECT 61,'statement','stmk_expr','execution'
 UNION ALL SELECT 150,'expression','enk_constant','metadata') a JOIN kind k ON k.family=a.f AND k.name=a.n;
INSERT INTO expression(id,type_id,value_category_id) SELECT id,3,
 (SELECT id FROM kind WHERE family='value_category' AND name='prvalue') FROM
 (SELECT 11 id UNION ALL SELECT 12 UNION ALL SELECT 13 UNION ALL SELECT 60 UNION ALL SELECT 150);
INSERT INTO statement(id,kind_id,expression_id,parent_id) SELECT a.id,k.id,expr,parent FROM
 (SELECT 10 id,'stmk_block' n,NULL expr,NULL parent UNION ALL SELECT 14,'stmk_expr',11,10
 UNION ALL SELECT 15,'stmk_return',13,10 UNION ALL SELECT 61,'stmk_expr',60,10) a
 JOIN kind k ON k.family='statement' AND k.name=a.n;
INSERT INTO block_descriptor(id,scope_id,lifetime_id) VALUES(81,9,80);
INSERT INTO block_statement(id,descriptor_id) VALUES(10,81);
INSERT INTO block_item VALUES(10,0,14),(10,1,61),(10,2,15);
INSERT INTO structural_child(owner_id,role_id,ordinal,child_id) SELECT owner,k.id,ordinal,child FROM
 (SELECT 10 owner,'statement' n,0 ordinal,14 child UNION ALL SELECT 10,'statement',1,61 UNION ALL SELECT 10,'statement',2,15
 UNION ALL SELECT 14,'operand',0,11 UNION ALL SELECT 11,'callee',0,12 UNION ALL SELECT 11,'argument',0,13
 UNION ALL SELECT 15,'return_value',0,13 UNION ALL SELECT 61,'operand',0,60) a
 JOIN kind k ON k.family='role' AND k.name=a.n;
INSERT INTO operator_expression(id,operator_kind_id) VALUES(11,(SELECT id FROM kind WHERE family='operator' AND name='eok_call'));
INSERT INTO call_expression(id,callee_id,target_routine_id,function_type_id,call_kind_id) VALUES
 (11,12,20,4,(SELECT id FROM kind WHERE family='call_kind' AND name='direct'));
INSERT INTO call_argument(owner_id,ordinal,expression_id) VALUES(11,0,13);
INSERT INTO reference_expression(id,target_id,reference_kind_id) VALUES(12,20,(SELECT id FROM kind WHERE family='reference_kind' AND name='routine'));
INSERT INTO semantic_reference(owner_id,role_id,ordinal,target_id) VALUES(12,(SELECT id FROM kind WHERE family='role' AND name='callee'),0,20);
INSERT INTO constant_value(id,kind_id) VALUES
 (31,(SELECT id FROM kind WHERE family='constant' AND name='ck_integer')),
 (100,(SELECT id FROM kind WHERE family='constant' AND name='ck_float')),
 (103,(SELECT id FROM kind WHERE family='constant' AND name='ck_string'));
INSERT INTO integer_value(id,value_id) VALUES(31,11);
INSERT INTO constant(id,type_id,value_id,backing_expression_id) VALUES(30,3,31,150),(101,102,100,NULL),(104,105,103,NULL);
INSERT INTO constant_expression(id,constant_id) VALUES(13,30),(150,30);
INSERT INTO floating_value(id,class_id,negative,radix,significand_id,exponent)
 VALUES(100,(SELECT id FROM kind WHERE family='float_class' AND name='zero'),1,2,21,0);
INSERT INTO string_value(id,element_bits,length,encoding_id,unit_kind_id)
 VALUES(103,16,'3',(SELECT id FROM kind WHERE family='string_encoding' AND name='target_code_units'),(SELECT id FROM kind WHERE family='string_unit_kind' AND name='target_characters'));
INSERT INTO string_unit VALUES(103,0,65),(103,1,0),(103,2,65535);
INSERT INTO jump_statement(id,kind_id,value_id) VALUES(15,(SELECT id FROM kind WHERE family='jump_kind' AND name='return'),13);
INSERT INTO record_type(id,definition_state_id,instance_id)
 VALUES(40,(SELECT id FROM kind WHERE family='definition_state' AND name='complete'),92);
INSERT INTO field_symbol(id,record_type_id,type_id) VALUES(42,40,3);
INSERT INTO record_layout(id,type_id,is_complete) VALUES(44,40,1);
INSERT INTO field_layout(owner_id,ordinal,field_id,offset) VALUES(44,0,42,'0');
INSERT INTO function_eh(id,routine_id,prologue_id,initial_state,null_state)
 VALUES(50,6,60,'18446744073709551615','18446744073709551615');
INSERT INTO cleanup_region(id,function_eh_id,ordinal,condition_id) VALUES(51,50,0,55);
INSERT INTO cleanup_storage_location(id,kind_id,variable_id,offset)
 VALUES(53,(SELECT id FROM kind WHERE family='storage_location_kind' AND name='direct_variable'),7,'0');
INSERT INTO type(id,kind_id,size,alignment,incomplete) VALUES(84,(SELECT id FROM kind WHERE family='type' AND name='tk_pointer'),'8','8',0);
INSERT INTO pointer_type(id,pointee_type_id) VALUES(84,3);
INSERT INTO constant_value(id,kind_id) VALUES(83,(SELECT id FROM kind WHERE family='constant' AND name='ck_address'));
INSERT INTO address_value(id,base_kind_id,base_entity_id,offset) VALUES(83,(SELECT id FROM kind WHERE family='address_base' AND name='abk_variable'),7,'0');
INSERT INTO constant(id,type_id,value_id) VALUES(82,84,83);
INSERT INTO cleanup_array(id,function_eh_id,ordinal,handle_constant_id,element_size,element_count) VALUES(54,50,0,82,'16','-1');
INSERT INTO cleanup_condition(id,storage_id,kind_id,comparison_value)
 VALUES(55,53,(SELECT id FROM kind WHERE family='condition_kind' AND name='nonzero'),'0');
INSERT INTO cleanup_abi_detail(id,is_delegating,vtt_location_id) VALUES(56,1,53);
INSERT INTO cleanup_action(id,region_id,ordinal,kind_id,routine_id,storage_id,array_id,abi_detail_id,is_array,is_conditional)
 VALUES(52,51,0,(SELECT id FROM kind WHERE family='cleanup_kind' AND name='destructor'),20,53,54,56,1,1);
INSERT INTO lowered_eh_operation(id,kind_id,function_eh_id,region_id,state_number)
 VALUES(60,(SELECT id FROM kind WHERE family='lowered_eh' AND name='leck_cleanup_state'),50,51,'0');
INSERT INTO cleanup_state_operation(id,region_id,state_number) VALUES(60,51,'0');
INSERT INTO source_file(id,path_id,origin_kind_id)
 VALUES(74,12,(SELECT id FROM kind WHERE family='source_origin_kind' AND name='ordinary'));
INSERT INTO include_occurrence(id,file_id,ordinal,display_name_id,is_top_level) VALUES(96,74,0,12,1);
INSERT INTO source_position(id,sequence,column,original_sequence,original_column,macro_invocation_id,file_id,logical_line,origin_kind_id)
 VALUES(72,'10',3,'5',7,71,74,'10',(SELECT id FROM kind WHERE family='source_origin_kind' AND name='macro'));
INSERT INTO source_origin(id,position_id,kind_id)
 VALUES(73,72,(SELECT id FROM kind WHERE family='source_origin_kind' AND name='macro'));
INSERT INTO macro_definition(id,name_id,text_id,kind_id,is_function_like)
 VALUES(70,13,14,(SELECT id FROM kind WHERE family='source_origin_kind' AND name='ordinary'),1);
INSERT INTO macro_invocation(id,definition_id,position_id,argument_text_id,ordinal) VALUES(71,70,72,15,0);
INSERT INTO lifetime(id,kind_id,bound_entity_id)
 VALUES(80,(SELECT id FROM kind WHERE family='lifetime' AND name='olk_block'),81);
INSERT INTO symbol(id,kind_id,name_id,type_id)
 VALUES(95,(SELECT id FROM kind WHERE family='entity' AND name='template_symbol'),41,40),
       (93,(SELECT id FROM kind WHERE family='entity' AND name='template_parameter'),94,3);
INSERT INTO template_definition(id,kind_id,pattern_id)
 VALUES(90,(SELECT id FROM kind WHERE family='template' AND name='templk_class'),40);
INSERT INTO template_symbol(id,descriptor_id) VALUES(95,90);
INSERT INTO template_argument(id,kind_id,type_id)
 VALUES(91,(SELECT id FROM kind WHERE family='template_argument' AND name='tak_type'),3);
INSERT INTO template_instance(id,result_id,template_id,state_id)
 VALUES(92,40,90,(SELECT id FROM kind WHERE family='instantiation_state' AND name='concrete'));
INSERT INTO template_instance_argument VALUES(92,(SELECT id FROM kind WHERE family='template_argument_role' AND name='primary'),0,91);
INSERT INTO template_parameter(id,template_id,ordinal,depth,kind_id)
 VALUES(93,90,0,0,(SELECT id FROM kind WHERE family='template_parameter' AND name='tpk_type'));
