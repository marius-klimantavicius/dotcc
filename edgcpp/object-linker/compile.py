#!/usr/bin/env python3
"""Compile one original source into a fresh owned SQLite semantic .dotobj."""
import argparse
import hashlib
import json
import os
import re
from pathlib import Path
import sqlite3
import subprocess
import time
import types
import zlib

ROOT = Path(__file__).resolve().parents[1]
SCHEMA = Path(__file__).with_name('schema.sql')
ARRAYS = {
    'edgGraphNodes': ('edg_graph_nodes', ['id', 'entryKind', 'entryKindName', 'capturePhase']),
    'edgGraphFieldDefinitions': ('edg_graph_field_definitions', ['id', 'ownerEntryKind', 'name', 'valueKind', 'targetEntryKind']),
    'edgGraphFieldBlocks': ('edg_graph_field_blocks', ['blockOrdinal', 'rowCount', 'dataHex', 'encoding']),
    'edgGraphRoots': ('edg_graph_roots', ['nodeId', 'contextNodeId', 'role', 'ordinal']),
    'edgGraphCoverage': ('edg_graph_coverage', ['entryKind', 'field', 'disposition', 'detail']),
    'sourceNamespaces': ('source_namespaces', ['id','parentId','sourceName','isUnnamed','isInline','hasInternalLinkage','sourceFileId','line','column','captureProof']),
    'sourceNamespaceOwners': ('source_namespace_owners', ['ownerKind','ownerId','namespaceId','captureProof']),
    'sourceEnumIncrements': ('source_enum_increments', ['operationConstantId', 'previousConstantId', 'oneConstantId', 'originalSourceTypeId', 'didForceDependentType', 'captureProof']),
    'sourceSemanticConstantExpressions': ('source_semantic_constant_expressions', ['constantId', 'expressionId', 'captureProof']),
    'sourceProxyTypes': ('source_proxy_types', ['sourceRecordId', 'originalTypeId', 'sourceOccurrence', 'captureProof']),
    'sourcePackSizes': ('source_pack_sizes', ['constantId', 'isType', 'isTemplateTemplate', 'sourceTypeId', 'expressionId', 'templateId', 'captureProof']),
    'sourceAggregateConstants': ('source_aggregate_constants', ['constantId', 'elementCount', 'originalTypeId', 'backingExpressionId', 'flagBits', 'addedTemplateConst', 'captureProof']),

    'sourceConstraintArgumentDetails': ('source_constraint_argument_details', ['ownerKind', 'ownerId', 'substitutionOrdinal', 'argumentOrdinal', 'isPack', 'isPackElement', 'hasPackEllipsis', 'isIntegerPack', 'isExplicit', 'isInjectedClassName', 'parameterIsAuto', 'parameterIsDecltypeAuto', 'packExpansionId', 'substitutedTemplateId', 'captureProof']),
    'sourcePackExpansions': ('source_pack_expansions', ['id', 'referenceCount', 'ellipsisSeen', 'usesOnlyEnclosingPacks', 'usesAnyEnclosingPacks', 'isFunctionDeclarator', 'isPackIndex', 'parameterIsTemplateTemplate', 'parameterSourceTypeId', 'sourceFileId', 'line', 'column', 'captureProof']),
    'sourcePackReferences': ('source_pack_references', ['expansionId', 'ordinal', 'originalKind', 'kind', 'sourceTypeId', 'constantId', 'templateId', 'parameterDepth', 'parameterPosition', 'usesEnclosingPack', 'sourceFileId', 'line', 'column']),
    'sourceDependentFunctionLookups': ('source_dependent_function_lookups', ['expressionId', 'sourceName', 'qualifiedName', 'ownerSourceTypeId', 'conversionSourceTypeId', 'operatorKind', 'hasExplicitTemplateArguments', 'isQualifiedName', 'hasAddressOf', 'hasLookupSymbol', 'candidateCount', 'memberOfUnknownBase', 'qualifiedUnknownBaseMember', 'captureProof']),
    'sourceDependentFunctionCandidates': ('source_dependent_function_candidates', ['expressionId', 'ordinal', 'declarationId']),
    'sourceDependentFunctionArguments': ('source_dependent_function_arguments', ['expressionId', 'ordinal', 'kind', 'sourceTypeId', 'constantId', 'templateId']),

    'sourceConcepts': ('source_concepts', ['hasNamespaceOwner', 'id', 'sourceName', 'qualifiedName', 'parameterCount', 'definitionExpressionId', 'headerConstraintExpressionId', 'sourceFileId', 'line', 'column', 'captureProof']),
    'sourceConceptParameters': ('source_concept_parameters', ['conceptId', 'ordinal', 'kind', 'sourceName', 'typeId', 'constantId', 'templateId', 'defaultTypeId', 'defaultConstantId', 'defaultTemplateId', 'isPack', 'isAbbreviated', 'sourceFileId', 'line', 'column']),
    'sourceExpressionTemporaries': ('source_expression_temporaries', ['expressionId', 'initializationKind', 'spelledSourceTypeId', 'constructorDeclarationId', 'destructorDeclarationId', 'constantId', 'isDependentConstructor', 'lifetimeDisposition', 'captureProof']),
    'sourceExpressionTemporaryFlags': ('source_expression_temporary_flags', ['expressionId', 'flag']),
    'sourceExpressionAllocations': ('source_expression_allocations', ['expressionId', 'objectSourceTypeId', 'operatorDeclarationId', 'isNew', 'isDependentSelection', 'isPlacement', 'isAligned', 'isArrayDelete', 'isGlobal', 'hasInitializer', 'isBracedInitializer', 'isParenthesizedAggregate', 'hasDeducibleType', 'hasParenthesizedTypeId', 'initializationDisposition', 'rollbackDisposition', 'captureProof', 'isLookupDeferred']),
    'sourceLocalScopes': ('source_local_scopes', ['id','routineDeclarationId','parentId','kind','siblingOrdinal','childScopeCount','variableCount','sourceFileId','line','column','captureProof']),
    'sourceLocalVariableDeclarations': ('source_local_variable_declarations', ['declarationId','scopeId','declarationOrdinal','declaredSourceTypeId','captureProof']),
    'sourceFunctionParameterDeclarations': ('source_function_parameter_declarations', ['declarationId', 'routineDeclarationId', 'binderId', 'parameterOrdinal', 'declaredSourceTypeId', 'captureProof']),
    'sourceDeclarationOperators': ('source_declaration_operators', ['declarationId', 'operatorKind', 'captureProof']),
    'sourceExpressionFieldReferences': ('source_expression_field_references', ['expressionId', 'sourceFieldId', 'captureProof']),
    'sourceExpressionFolds': ('source_expression_folds', ['expressionId', 'operatorToken', 'operatorSpelling', 'isLeftAssociative', 'captureProof']),
    'sourceExpressions': ('source_expressions', ['id', 'kind', 'edgeCount', 'parameterCount', 'argumentCount', 'substitutionCount', 'originalLvalueTypeId', 'isParenthesized', 'isCompilerGenerated', 'isVolatileFetch', 'isPackExpansion', 'sourceTypeId', 'valueCategory', 'sourceFileId', 'line', 'column', 'captureProof']),
    'sourceExpressionEdges': ('source_expression_edges', ['expressionId', 'role', 'ordinal', 'childExpressionId']),
    'sourceTypeConstraints': ('source_type_constraints', ['sourceTypeId', 'constraintExpressionId', 'captureProof']),
    'sourceConceptUses': ('source_concept_uses', ['expressionId', 'conceptId', 'isTypeConstraint']),
    'sourceConceptArguments': ('source_concept_arguments', ['expressionId', 'ordinal', 'kind', 'sourceTypeId', 'constantId', 'templateId']),
    'sourceExpressionParameters': ('source_expression_parameters', ['expressionId', 'ordinal', 'sourceTypeId', 'declaredSourceTypeId', 'sourceName', 'qualifierBits', 'isPack']),
    'sourceExpressionParameterReferences': ('source_expression_parameter_references', ['expressionId', 'binderId', 'parameterNumber', 'originalLevelsUp']),
    'sourceExpressionOperations': ('source_expression_operations', ['expressionId', 'opcode', 'operationName', 'originalTypeKind']),
    'sourceExpressionFlags': ('source_expression_flags', ['expressionId', 'flag']),
    'sourceExpressionTypeOperands': ('source_expression_type_operands', ['expressionId', 'sourceTypeId']),
    'sourceExpressionConstants': ('source_expression_constants', ['expressionId', 'constantId']),
    'sourceRoutinePrototypeOrigins': ('source_routine_prototype_origins', ['routineSourceTypeId', 'originalRoutineSourceTypeId', 'originalBinderId', 'kind', 'captureProof']),
    'sourceParameterBinders': ('source_parameter_binders', ['id', 'kind', 'parentBinderId', 'requiresExpressionId', 'routineSourceTypeId', 'outsideParameterList', 'parameterCount', 'captureProof']),
    'sourceBinderParameters': ('source_binder_parameters', ['binderId', 'ordinal', 'sourceTypeId', 'declaredSourceTypeId', 'sourceName', 'qualifierBits', 'isPack']),
    'sourceExpressionBindings': ('source_expression_bindings', ['expressionId', 'lexicalBinderId']),
    'sourceExpressionRequirements': ('source_expression_requirements', ['expressionId', 'requiresNoexcept']),
    'sourceExpressionQueries': ('source_expression_queries', ['expressionId', 'isStandardAlignof']),
    'sourceExpressionSubstitutions': ('source_expression_substitutions', ['expressionId', 'substitutionOrdinal', 'formalCount', 'argumentCount', 'argumentsKnownDependent', 'argumentsKnownNondependent', 'adjustCoordinates', 'aliasDeductionGuide']),
    'sourceExpressionSubstitutionFormals': ('source_expression_substitution_formals', ['expressionId', 'substitutionOrdinal', 'ordinal', 'kind', 'sourceName', 'typeId', 'constantId', 'templateId', 'defaultTypeId', 'defaultConstantId', 'defaultTemplateId', 'isPack', 'isAbbreviated', 'sourceFileId', 'line', 'column']),
    'sourceExpressionSubstitutionArguments': ('source_expression_substitution_arguments', ['expressionId', 'substitutionOrdinal', 'ordinal', 'kind', 'sourceTypeId', 'constantId', 'templateId']),
    'sourceExpressionDeclarations': ('source_expression_declarations', ['id', 'kind', 'sourceName', 'qualifiedName', 'sourceTypeId', 'ownerSourceTypeId', 'templatePrototypeDeclarationId', 'templateOriginKind', 'routineKind', 'scopeKind', 'hasTemplateOrigin', 'isTemplatePrototype', 'isConstexpr', 'formalCount', 'argumentCount', 'originalInitKind', 'originalDynamicInitKind', 'initializerKind', 'initializerConstantId', 'initializerExpressionId', 'definitionStatus', 'captureProof', 'sourceFileId', 'line', 'column']),
    'sourceExpressionDeclarationReferences': ('source_expression_declaration_references', ['expressionId', 'declarationId']),
    'sourceExpressionDeclarationFormals': ('source_expression_declaration_formals', ['declarationId', 'ordinal', 'kind', 'sourceName', 'typeId', 'constantId', 'templateId', 'defaultTypeId', 'defaultConstantId', 'defaultTemplateId', 'isPack', 'isAbbreviated', 'sourceFileId', 'line', 'column']),
    'sourceExpressionDeclarationArguments': ('source_expression_declaration_arguments', ['declarationId', 'ordinal', 'kind', 'sourceTypeId', 'constantId', 'templateId']),

    'sourceNestedTypeDeclarations': ('source_nested_type_declarations', ['typeId', 'ownerTypeId', 'captureProof']),
    'sourceDependentTypes': ('source_dependent_types', ['typeId', 'kind', 'ownerTypeId', 'originalNestedTypeId', 'nestingDepth', 'parameterPosition', 'isPack', 'identityKey', 'captureProof', 'hasTypeConstraint']),
    'sourceNamedTemplates': ('source_named_templates', ['hasNamespaceOwner', 'templateId', 'qualifiedName', 'ownerSourceTypeId', 'prototypeSourceTypeId', 'constraintExpressionId', 'captureProof']),
    'sourceAliasInstances': ('source_alias_instances', ['typeId', 'templateId', 'isPrototype', 'isNonreal', 'isDependent', 'argumentCount', 'hasOriginalArguments', 'originalArgumentCount', 'minimumArguments', 'captureProof']),
    'sourceAliasArguments': ('source_alias_arguments', ['typeId', 'listKind', 'ordinal', 'kind', 'sourceTypeId', 'constantId', 'templateId']),
    'sourceTypeOperators': ('source_type_operators', ['typeId', 'opcode', 'expressionId', 'operandSourceTypeId', 'isDependent', 'isUnparenthesizedDecltype', 'captureProof', 'operatorName']),
    'sourceFiles': ('source_files', ['id', 'path', 'parentId']),
    'types': ('types', ['id', 'kind', 'pointeeTypeId', 'elementTypeId', 'elementCount']),
    'storages': ('storages', ['id', 'sourceName', 'linkage', 'externalKey', 'definition', 'typeId', 'sourceTypeId', 'isThreadLocal', 'isConst', 'alignment', 'sourceFileId', 'line', 'column']),
    'storageData': ('storage_data', ['storageId', 'byteOffset', 'dataHex']),
    'relocations': ('relocations', ['storageId', 'byteOffset', 'targetStorageId', 'targetEntityId', 'addend']),
    'routineTypes': ('routine_types', ['typeId', 'returnTypeId', 'variadic', 'exceptionPolicy']),
    'routineParameters': ('routine_parameters', ['typeId', 'ordinal', 'parameterTypeId']),
    'sourceRoutineExceptionResolutions': ('source_routine_exception_resolutions', ['sourceRoutineTypeId', 'finalSourceRoutineTypeId', 'sourceFunctionEntityId', 'initialPolicy', 'resolvedPolicy', 'initialPresent', 'initialIndeterminate', 'initialArgCached', 'initialCopyFromPrototype', 'initialIsNoexcept', 'initialThrowAny', 'finalPresent', 'finalIndeterminate', 'finalArgCached', 'finalCopyFromPrototype', 'finalIsNoexcept', 'finalThrowAny', 'specialMemberKind', 'isDefaulted', 'isCompilerGenerated', 'isInheritingConstructor', 'captureProof']),
    'sourceDynamicExceptionSpecifications': ('source_dynamic_exception_specifications', ['sourceRoutineTypeId', 'kind', 'allowedTypeCount', 'captureProof']),
    'sourceDynamicExceptionTypes': ('source_dynamic_exception_types', ['sourceRoutineTypeId', 'ordinal', 'declaredSourceTypeId', 'matchSourceTypeId']),
    'functionDynamicExceptionBoundaries': ('function_dynamic_exception_boundaries', ['functionEntityId', 'sourceRoutineTypeId', 'captureProof']),
    'sourceRoutineTypes': ('source_routine_types', ['typeId', 'returnTypeId', 'variadic', 'exceptionPolicy', 'parameterMode', 'objectTypeId', 'objectIsConst', 'objectIsVolatile', 'objectRefQualifier', 'exceptionPolicyOrigin', 'prototypeOrigin', 'hasBorrowedArguments', 'declaratorContextCount']),
    'sourceDeclaratorContexts': ('source_declarator_contexts', ['id', 'routineSourceTypeId', 'scopeOrdinal', 'kind', 'selectedSourceDeclarationId', 'sourceRecordId', 'headerSourceRecordId', 'argumentCount', 'borrowedArgumentCount', 'sourceFileId', 'line', 'column', 'captureProof']),
    'sourceDeclaratorBorrowedArguments': ('source_declarator_borrowed_arguments', ['contextId', 'argumentOrdinal', 'sourceTypeId']),
    'sourceRoutineTypeSubstitutions': ('source_routine_type_substitutions', ['routineSourceTypeId', 'selectedSourceDeclarationId', 'argumentOwnerKind', 'borrowedArgumentCount', 'captureProof']),
    'sourceRoutineBorrowedArguments': ('source_routine_borrowed_arguments', ['routineSourceTypeId', 'argumentOrdinal', 'sourceTypeId']),
    'sourceRoutineParameters': ('source_routine_parameters', ['typeId', 'ordinal', 'parameterTypeId']),
    'deferredFunctionDeclarations': ('deferred_function_declarations', ['entityId', 'sourceRoutineTypeId', 'captureProof']),
    'deferredRoutineAbis': ('deferred_routine_abis', ['routineTypeId', 'sourceRoutineTypeId', 'reason', 'captureProof']),
    'sourceRoutineAbis': ('source_routine_abis', ['sourceRoutineTypeId', 'returnPassing', 'captureProof']),
    'sourceRoutineAbiParameters': ('source_routine_abi_parameters', ['sourceRoutineTypeId', 'executionOrdinal', 'parameterRole', 'sourceOrdinal']),
    'entities': ('entities', ['id', 'sourceName', 'linkage', 'externalKey', 'definition',
                              'returnTypeId', 'sourceFileId', 'line', 'column', 'routineTypeId', 'isWeak']),
    'parameters': ('parameters', ['entityId', 'ordinal', 'typeId', 'parameterRole', 'sourceOrdinal']),
    'functions': ('functions', ['entityId', 'bodyNodeId']),
    'abiRoutineOrigins': ('abi_routine_origins', ['entityId', 'primaryEntityId', 'entryKind', 'isAliasEntry', 'captureProof', 'targetEntityId']),
    'compileTimeSelections': ('compile_time_selections', ['nodeId', 'conditionValue', 'hasElse', 'captureProof']),
    'thunkWrappers': ('thunk_wrappers', ['entityId', 'targetEntityId', 'overriddenEntityId', 'primaryEntityId', 'thisDelta', 'vcallIndex', 'returnDelta', 'vbaseIndex', 'captureProof']),
    'wrapperCalls': ('wrapper_calls', ['nodeId', 'wrapperEntityId', 'captureProof']),
    'lambdaEntryOrigins': ('lambda_entry_origins', ['entityId', 'targetEntityId', 'closureSourceTypeId', 'entrySourceRoutineTypeId', 'captureProof']),
    'lambdaEntryCalls': ('lambda_entry_calls', ['nodeId', 'entryEntityId', 'captureProof']),
    'initializationActions': ('initialization_actions', ['id', 'entityId', 'phase', 'priority', 'ordinal', 'sourceFileId', 'line', 'column']),
    'sourceInitializers': ('source_initializers', ['id', 'storageId', 'sourceName', 'executionDisposition', 'sourceTypeId', 'functionEntityId', 'sourceOrdinal', 'sourceFieldId', 'kind', 'sourceFileId', 'line', 'column', 'captureProof']),
    'sourceFieldInitializerDestructions': ('source_field_initializer_destructions', ['sourceInitializerId', 'sourceTypeId', 'selectedRoutineEntityId', 'captureProof']),
    'fieldInitializerExecutions': ('field_initializer_executions', ['sourceInitializerId', 'functionEntityId', 'constructorOrdinal', 'executionDisposition', 'captureProof']),
    'fieldInitializerDestructionElisions': ('field_initializer_destruction_elisions', ['sourceInitializerId', 'functionEntityId', 'kind', 'captureProof']),
    'fieldInitializerPartialDestructionExecutions': ('field_initializer_partial_destruction_executions', ['sourceStaticObligationId', 'functionEntityId', 'kind', 'captureProof']),
    'sourceStaticLifetimes': ('source_static_lifetimes', ['id', 'functionEntityId', 'sourceInitializerId', 'parentLifetimeId', 'parentObligationId', 'siblingOrdinal', 'kind', 'sourceFileId', 'line', 'column', 'captureProof']),
    'sourceStaticDestructions': ('source_static_destructions', ['id', 'lifetimeId', 'ordinal', 'storageId', 'sourceTypeId', 'selectedRoutineEntityId', 'sourceInitializerId', 'kind', 'isArrayAllocation', 'domain', 'sourceFileId', 'line', 'column', 'captureProof']),
    'initializerLifetimeExecutions': ('initializer_lifetime_executions', ['sourceLifetimeId', 'sourceStaticLifetimeId', 'sourceInitializerId', 'functionEntityId', 'captureProof']),
    'staticDestructionRegistrations': ('static_destruction_registrations', ['nodeId', 'sourceObligationId', 'sourceInitializerId', 'functionEntityId', 'callbackEntityId', 'objectNodeId', 'domain', 'captureProof']),
    'defaultArgumentEvaluations': ('default_argument_evaluations', ['functionEntityId', 'ownerFunctionEntityId', 'sourceFunctionEntityId', 'targetEntityId', 'forwardingCallNodeId', 'rootSourceLifetimeId', 'captureProof']),
    'defaultArgumentLifetimeExecutions': ('default_argument_lifetime_executions', ['functionEntityId', 'sourceLifetimeId', 'copiedParentSourceLifetimeId', 'copiedSiblingOrdinal', 'captureProof']),
    'defaultArgumentObligationExecutions': ('default_argument_obligation_executions', ['functionEntityId', 'sourceObligationId', 'sourceAllocationObligationId', 'sourceLifetimeId', 'copiedOrdinal', 'captureProof']),
    'defaultArgumentTemporaryExecutions': ('default_argument_temporary_executions', ['functionEntityId', 'sourceTemporaryId', 'captureProof']),
    'tlsRoutineOrigins': ('tls_routine_origins', ['entityId', 'kind', 'sourceStorageId', 'targetEntityId', 'guardKind', 'captureProof']),
    'generatedEntityInitializers': ('generated_entity_initializers', ['functionEntityId','ownerFunctionEntityId','sourceAllocationObligationId','sourceElementTypeId','elementTypeId','destinationBindingId','countBindingId','modelBindingId','copyNodeId','captureProof']),
    'generatedEntityInitializerCalls': ('generated_entity_initializer_calls', ['nodeId','helperEntityId','captureProof']),
    'generatedArrayInitializers': ('generated_array_initializers', ['functionEntityId', 'ownerFunctionEntityId', 'sourceAllocationObligationId', 'sourceElementTypeId', 'elementTypeId', 'constructorEntityId', 'objectParameterBindingId', 'modelBindingId', 'modelInitializationNodeId', 'modelCopyNodeId', 'constructorCallNodeId', 'modelLoweredInitKind', 'modelSiteKind', 'modelZeroStorage', 'modelPartiallyInitialized', 'captureProof']),
    'generatedStaticDestructors': ('generated_static_destructors', ['functionEntityId', 'sourceObligationId', 'parameterBindingId', 'storageId', 'elementTypeId', 'elementCount', 'elementSize', 'captureProof']),
    'staticDestructionDispositions': ('static_destruction_dispositions', ['sourceObligationId', 'kind', 'captureProof']),
    'sourceLifetimes': ('source_lifetimes', ['id', 'functionEntityId', 'parentLifetimeId', 'siblingOrdinal', 'parentObligationId', 'parentAllocationObligationId', 'kind', 'sourceFileId', 'line', 'column', 'captureProof', 'sourceInitializerId']),
    'sourceDestructionObligations': ('source_destruction_obligations', ['id', 'functionEntityId', 'lifetimeId', 'ordinal', 'destructorEntityId', 'sourceTypeId', 'sourceFileId', 'line', 'column', 'captureProof', 'sourceInitializerId', 'sourceStaticObligationId', 'isPartialAggregate']),
    'destructionElisions': ('destruction_elisions', ['sourceObligationId', 'functionEntityId', 'kind', 'captureProof']),
    'sourceArrayDeletions': ('source_array_deletions', ['sourceObligationId', 'deallocatorEntityId', 'captureProof']),
    'arrayDeletionOperations': ('array_deletion_operations', ['nodeId', 'sourceObligationId', 'destructorEntityId', 'deallocatorEntityId', 'elementTypeId', 'paddingSize', 'helperKind', 'captureProof', 'callbackKind']),
    'arrayDeletionVirtualCallbacks': ('array_deletion_virtual_callbacks', ['callNodeId', 'callbackNodeId', 'vtableIndex', 'cookieSize', 'captureProof']),
    'sourceArrayAllocationElements': ('source_array_allocation_elements', ['sourceAllocationObligationId', 'sourceDestructionObligationId', 'captureProof']),
    'arrayAllocationOperations': ('array_allocation_operations', ['nodeId', 'sourceAllocationObligationId', 'sourceDestructionObligationId', 'allocatorEntityId', 'deallocatorEntityId', 'constructorEntityId', 'destructorEntityId', 'elementTypeId', 'paddingSize', 'helperKind', 'captureProof']),
    'sourceUninitializedAllocations': ('source_uninitialized_allocations', ['sourceAllocationObligationId', 'allocatorEntityId', 'isPlacement', 'hasInitializer', 'captureProof']),
    'allocationElisions': ('allocation_elisions', ['sourceAllocationObligationId', 'functionEntityId', 'kind', 'captureProof']),
    'sourceAllocationObligations': ('source_allocation_obligations', ['id', 'functionEntityId', 'lifetimeId', 'ordinal', 'deallocatorEntityId', 'allocatedSourceTypeId', 'isArray', 'sourceFileId', 'line', 'column', 'captureProof', 'sourceInitializerId', 'sourceStaticObligationId']),
    'internalTryExpressions': ('internal_try_expressions', ['nodeId', 'sourceAllocationObligationId', 'cleanupPolicy', 'captureProof']),
    'expressionLifetimes': ('expression_lifetimes', ['nodeId', 'sourceLifetimeId', 'captureProof']),
    'baseSubobjectExpressions': ('base_subobject_expressions', ['nodeId', 'sourceBaseId']),
    'virtualBaseExpressions': ('virtual_base_expressions', ['nodeId', 'completeBaseTypeId']),
    'virtualBaseSubobjects': ('virtual_base_subobjects', ['recordTypeId', 'completeBaseTypeId', 'physicalViewTypeId', 'byteOffset', 'extent', 'fieldId', 'captureProof']),
    'normalDestructions': ('normal_destructions', ['nodeId', 'sourceObligationId', 'captureProof', 'sourceStaticObligationId', 'sourceFieldInitializerId']),
    'allocationCleanupActions': ('allocation_cleanup_actions', ['id', 'functionEntityId', 'sourceAllocationObligationId', 'callbackEntityId', 'targetNodeId', 'allocationSize', 'conditionBindingId', 'nullPolicy', 'captureProof']),
    'localStaticGuards': ('local_static_guards', ['id', 'functionEntityId', 'storageId', 'guardStorageId', 'sourceTypeId', 'sourceLifetimeId', 'guardPolicy', 'sourceFileId', 'line', 'column', 'captureProof']),
    'guardOperations': ('guard_operations', ['nodeId', 'guardId', 'kind', 'captureProof']),
    'guardCleanupActions': ('guard_cleanup_actions', ['id', 'functionEntityId', 'guardId', 'targetNodeId', 'captureProof']),
    'cleanupActions': ('cleanup_actions', ['id', 'functionEntityId', 'sourceObligationId', 'sourceStaticObligationId', 'callbackEntityId', 'targetNodeId', 'captureProof', 'sourceFieldInitializerId']),
    'arrayRuntimeOperations': ('array_runtime_operations', ['nodeId', 'sourceObligationId', 'callbackEntityId', 'operationKind', 'captureProof', 'sourceStaticObligationId', 'sourceFieldInitializerId']),
    'cleanupArrays': ('cleanup_arrays', ['actionId', 'elementTypeId', 'elementCount', 'elementSize', 'captureProof']),
    'cleanupVttArguments': ('cleanup_vtt_arguments', ['actionId', 'bindingId', 'captureProof']),
    'cleanupConditions': ('cleanup_conditions', ['actionId', 'flagBindingId', 'captureProof']),
    'cleanupStates': ('cleanup_states', ['id', 'functionEntityId', 'actionId', 'nextStateId', 'handlerNodeId', 'allocationActionId', 'guardActionId']),
    'cleanupTransitions': ('cleanup_transitions', ['nodeId', 'stateId', 'captureProof', 'isUnreachable']),
    'cleanupFrameMarkers': ('cleanup_frame_markers', ['nodeId', 'kind', 'captureProof']),
    'generatedRoutineOrigins': ('generated_routine_origins', ['entityId', 'kind', 'captureProof', 'failurePolicy']),
    'dynamicInitializers': ('dynamic_initializers', ['id', 'actionId', 'storageId', 'sourceInitializerId', 'sourceOrdinal', 'ordinal', 'captureProof', 'sourceFileId', 'line', 'column']),
    'dynamicInitializerStatements': ('dynamic_initializer_statements', ['initializerId', 'ordinal', 'nodeId']),
    'generatedRoutineScaffolds': ('generated_routine_scaffolds', ['entityId', 'ordinal', 'nodeId', 'bindingId', 'kind', 'captureProof']),
    'bindings': ('bindings', ['id', 'functionId', 'kind', 'ordinal', 'typeId', 'sourceName',
                              'scopeNodeId', 'sourceFileId', 'line', 'column', 'isSynthetic']),
    'nodes': ('nodes', ['id', 'functionId', 'kind', 'typeId', 'entityId', 'bindingId',
                        'operator', 'constantText', 'valueCategory', 'evaluationOrder',
                        'sourceFileId', 'line', 'column', 'fieldId', 'storageId']),
    'recordTypes': ('record_types', ['typeId', 'identityKey', 'sourceName', 'size', 'alignment', 'sourceFileId', 'line', 'column', 'layoutKind', 'sourceRecordId', 'representationKind', 'valueCopyKind', 'captureProof', 'completeTypeId']),
    'recordFields': ('record_fields', ['id', 'recordTypeId', 'ordinal', 'typeId', 'sourceName', 'byteOffset', 'sourceTypeId', 'sourceFileId', 'line', 'column', 'bitOffset', 'bitWidth', 'bitSigned', 'sourceFieldId', 'fieldRole', 'sourceBaseId', 'observedAlignment']),
    'sourceRecordOwners': ('source_record_owners', ['sourceRecordId', 'ownerSourceRecordId', 'ownerFieldId', 'kind', 'captureProof']),
    'sourceTemplates': ('source_templates', ['id', 'kind', 'sourceName', 'parameterDepth', 'parameterPosition', 'isPack', 'parameterCount', 'captureProof', 'sourceFileId', 'line', 'column']),
    'sourceTemplateParameters': ('source_template_parameters', ['sourceTemplateId', 'ordinal', 'kind', 'sourceName', 'typeId', 'constantId', 'templateId', 'defaultTypeId', 'defaultConstantId', 'defaultTemplateId', 'isPack', 'isAbbreviated', 'sourceFileId', 'line', 'column']),
    'sourceClassTemplateHeaders': ('source_class_template_headers', ['sourceRecordId', 'primarySourceRecordId', 'formalCount', 'patternCount', 'constraintExpressionId', 'captureProof']),
    'sourceClassSpecializations': ('source_class_specializations', ['sourceRecordId', 'selectedSourceRecordId', 'selectedArgumentCount', 'captureProof']),
    'sourceClassTemplateFormals': ('source_class_template_formals', ['sourceRecordId', 'ordinal', 'kind', 'sourceName', 'typeId', 'constantId', 'templateId', 'defaultTypeId', 'defaultConstantId', 'defaultTemplateId', 'isPack', 'isAbbreviated', 'sourceFileId', 'line', 'column']),
    'sourceClassTemplatePatterns': ('source_class_template_patterns', ['sourceRecordId', 'ordinal', 'kind', 'typeId', 'constantId', 'templateId', 'isPack']),
    'sourceClassSpecializationArguments': ('source_class_specialization_arguments', ['sourceRecordId', 'ordinal', 'kind', 'typeId', 'constantId', 'templateId', 'isPack']),
    'sourceRecordInstantiations': ('source_record_instantiations', ['sourceRecordId', 'prototypeSourceRecordId', 'resolutionKind', 'sourceTemplateId', 'memberOwnerSourceTypeId', 'memberOfUnknownBase', 'qualifiedUnknownBaseMember', 'captureProof', 'hasSelectedPartialSpecialization']),
    'sourceTemplateArguments': ('source_template_arguments', ['sourceRecordId', 'ordinal', 'kind', 'typeId', 'constantId', 'templateId', 'isPack']),
    'sourceArrayBounds': ('source_array_bounds', ['typeId', 'constantId', 'captureProof']),
    'sourceSemanticConstants': ('source_semantic_constants', ['id', 'kind', 'sourceTypeId', 'sourceName', 'valueText', 'parameterDepth', 'parameterPosition', 'isPack', 'operatorName', 'ownerSourceTypeId', 'operandSourceTypeId', 'builtinOperatorKind', 'isStandardAlignof', 'sourceFileId', 'line', 'column']),
    'sourceSemanticConstantOperands': ('source_semantic_constant_operands', ['constantId', 'ordinal', 'operandId']),
    'sourceRecords': ('source_record_types', ['id', 'sourceTypeId', 'identityKey', 'sourceName', 'recordKind', 'isComplete', 'nativeSize', 'nativeAlignment', 'isTriviallyCopyable', 'capturePhase', 'sourceFileId', 'line', 'column', 'isPacked', 'maxMemberAlignment', 'explicitAlignment', 'layoutKnown', 'isDependent', 'isPrototypeInstantiation', 'navigationKind']),
    'exceptionTypes': ('exception_types', ['id', 'sourceTypeId', 'leafSourceTypeId', 'payloadTypeId', 'payloadSize', 'payloadAlignment', 'sourceDestructorEntityId', 'captureProof']),
    'exceptionBaseConversions': ('exception_base_conversions', ['exceptionTypeId', 'ordinal', 'baseSourceTypeId', 'byteOffset', 'isPublic', 'isAmbiguous', 'captureProof']),
    'throwExpressions': ('throw_expressions', ['nodeId', 'exceptionTypeId', 'destructorEntityId', 'initializationKind', 'captureProof']),
    'throwInitializationBoundaries': ('throw_initialization_boundaries', ['throwNodeId', 'cleanupPolicy', 'captureProof']),
    'throwCleanupTransitions': ('throw_cleanup_transitions', ['nodeId', 'throwNodeId', 'captureProof']),
    'cleanupPayloadTargets': ('cleanup_payload_targets', ['nodeId', 'actionId', 'throwNodeId', 'captureProof']),
    'throwPayloadSubobjects': ('throw_payload_subobjects', ['sourceObligationId', 'throwNodeId', 'completionPolicy', 'captureProof']),
    'throwPayloadPaths': ('throw_payload_paths', ['sourceObligationId', 'ordinal', 'fieldId', 'sourceBaseId']),
    'sourceTries': ('source_tries', ['nodeId', 'sourceLifetimeId', 'tryKind', 'captureProof']),
    'sourceHandlers': ('source_handlers', ['nodeId', 'tryNodeId', 'ordinal', 'matchKind', 'sourceTypeId', 'matchTypeId', 'parameterBindingId', 'captureProof']),
    'caughtObjectAddresses': ('caught_object_addresses', ['nodeId', 'handlerNodeId', 'captureProof']),
    'handlerMarkers': ('handler_markers', ['nodeId', 'ownerNodeId', 'kind', 'captureProof']),
    'throwObjectAddresses': ('throw_object_addresses', ['nodeId', 'throwNodeId', 'captureProof']),
    'throwMarkers': ('throw_markers', ['nodeId', 'throwNodeId', 'kind', 'captureProof']),
    'rethrowExpressions': ('rethrow_expressions', ['nodeId', 'captureProof']),
    'sourceRecordOperations': ('source_record_operations', ['sourceRecordId', 'bitwiseConstructionAllowed', 'bitwiseAssignmentAllowed', 'requiresReturnStorage', 'requiresParameterStorage', 'captureProof']),
    'virtualBaseAdjustments': ('virtual_base_adjustments', ['nodeId', 'operandNodeId', 'sourceNodeId', 'lookupSourceNodeId', 'derivedTypeId', 'completeBaseTypeId', 'offsetNodeId', 'vtableIndex', 'isCompleteObject', 'captureProof']),
    'recordConstantInitializations': ('record_constant_initializations', ['nodeId', 'sourceRecordId', 'storageId', 'initializationKind', 'captureProof']),
    'sourceRecordTemporaries': ('source_record_temporaries', ['id', 'functionEntityId', 'sourceInitializerId', 'sourceTypeId', 'sourceOrdinal', 'dynamicInitKind', 'sourceDestructionObligationId', 'sourceFileId', 'line', 'column', 'captureProof']),
    'recordTemporaryInitializations': ('record_temporary_initializations', ['nodeId', 'bindingId', 'sourceTemporaryId', 'loweredInitKind', 'siteKind', 'zeroStorage', 'partiallyInitialized', 'captureProof']),
    'recordStorageInitializations': ('record_storage_initializations', ['nodeId', 'bindingId', 'sourceDeclarationId', 'sourceTypeId', 'sourceInitKind', 'loweredInitKind', 'siteKind', 'zeroStorage', 'partiallyInitialized', 'captureProof']),
    'recordStorageCopies': ('record_storage_copies', ['nodeId', 'targetNodeId', 'valueNodeId', 'sourceRecordId', 'byteCount', 'kind', 'captureProof']),
    'recordValueTransfers': ('record_value_transfers', ['nodeId', 'kind', 'captureProof']),
    'sourceBitfieldWidths': ('source_bitfield_widths', ['sourceFieldId', 'constantId', 'declaredWidth', 'widthKind', 'captureProof']),
    'sourceFields': ('source_record_fields', ['id', 'sourceRecordId', 'ordinal', 'sourceTypeId', 'sourceName', 'isMutable', 'hasNoUniqueAddress', 'bitWidth', 'sourceFileId', 'line', 'column', 'hasInitializer', 'isPacked', 'explicitAlignment']),
    'sourceBases': ('source_record_bases', ['id', 'sourceRecordId', 'ordinal', 'baseSourceTypeId', 'accessKind', 'isVirtual', 'sourceFileId', 'line', 'column']),
    'recordBaseSubobjects': ('record_base_subobjects', ['recordTypeId', 'sourceBaseId', 'completeBaseTypeId', 'physicalViewTypeId', 'byteOffset', 'extent', 'fieldId']),
    'memoryBarriers': ('memory_barriers', ['nodeId', 'isVolatile', 'hasVolatileKeyword', 'hasInlineKeyword', 'clobbersMemory', 'captureProof']),
    'memoryBarrierOperands': ('memory_barrier_operands', ['nodeId', 'ordinal', 'direction', 'constraintText', 'sourceName', 'sourceElementTypeId', 'elementSize', 'sourceShape', 'extentKind', 'fixedElementCount', 'extentBindingId', 'captureProof', 'sourceFileId', 'line', 'column']),
    'memoryBarrierDimensions': ('memory_barrier_dimensions', ['nodeId', 'operandOrdinal', 'ordinal', 'kind', 'fixedCount', 'extentBindingId', 'sourceFileId', 'line', 'column']),
    'edges': ('edges', ['parentId', 'role', 'ordinal', 'childId']),
    'macroDefinitions': ('macro_definitions', ['id', 'name', 'form', 'variadic', 'dynamic',
                        'definitionText', 'origin', 'sourceFileId', 'line', 'column']),
    'macroParameters': ('macro_parameters', ['definitionId', 'ordinal', 'name']),
    'macroEvents': ('macro_events', ['id', 'kind', 'name', 'definitionId', 'origin', 'sourceFileId', 'line', 'column']),
    'sourceTypes': ('source_types', ['hasNamespaceOwner', 'id', 'sourceForm', 'kind', 'sourceName', 'underlyingTypeId',
                                   'executionTypeId', 'isConst', 'isVolatile', 'sourceFileId', 'line', 'column', 'elementCount', 'isRestrict', 'isAtomic', 'arrayBoundKind']),
    'sourceTypeQualifiers': ('source_type_qualifiers', ['typeId', 'name']),
    'sourceEnumTypes': ('source_enum_types', ['typeId', 'identityKey', 'underlyingTypeId', 'isScoped', 'isComplete', 'enumeratorListComplete', 'hasExplicitBase', 'isTemplate', 'ownerTypeId', 'sourceOccurrence', 'enumeratorCount', 'isNonreal', 'isPrototype', 'isSpecialized', 'originallyUnnamed', 'underlyingOrigin']),
    'sourceEnumerators': ('source_enumerators', ['typeId', 'ordinal', 'sourceName', 'valueText', 'sourceFileId', 'line', 'column', 'constantId']),
    'sourceMemberPointerTypes': ('source_member_pointer_types', ['typeId', 'ownerTypeId', 'memberTypeId', 'captureProof']),
    'sourceFunctions': ('source_functions', ['entityId', 'returnSourceTypeId', 'capturePhase', 'routineSourceTypeId', 'functionKind']),
    'sourceParameters': ('source_parameters', ['entityId', 'ordinal', 'sourceTypeId', 'hasDefaultArgument']),
    'sourceDeclarations': ('source_declarations', ['id', 'kind', 'sourceName', 'sourceTypeId',
                          'entityId', 'bindingId', 'functionEntityId', 'sourceFileId', 'line', 'column']),
}


class FrontendOption(argparse.Action):
    """Retain preprocessing option order, including interleaved -D and -U."""
    def __call__(self, parser, namespace, values, option_string=None):
        options = list(getattr(namespace, self.dest, None) or [])
        options.append((option_string, values))
        setattr(namespace, self.dest, options)


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError('Duplicate wire key: ' + key)
        result[key] = value
    return result


class WireReader:
    """Read one wire row at a time; keep JSON syntax/duplicate-key checks."""
    def __init__(self, stream, chunk_size=65536):
        self.stream = stream
        self.chunk_size = chunk_size
        self.buffer = ''
        self.offset = 0
        self.eof = False
        self.decoder = json.JSONDecoder(object_pairs_hook=unique_object)

    def fill(self):
        self.buffer = self.buffer[self.offset:]
        self.offset = 0
        block = self.stream.read(self.chunk_size)
        self.eof = not block
        self.buffer += block
        return bool(block)

    def peek(self):
        while True:
            while self.offset < len(self.buffer) and self.buffer[self.offset] in ' \t\r\n':
                self.offset += 1
            if self.offset < len(self.buffer):
                return self.buffer[self.offset]
            if self.eof or not self.fill():
                return ''

    def take(self, expected):
        if self.peek() != expected:
            raise ValueError('Expected JSON ' + repr(expected))
        self.offset += 1

    def value(self):
        self.peek()
        while True:
            try:
                value, end = self.decoder.raw_decode(self.buffer, self.offset)
                # A scalar token may end at a chunk boundary before its next
                # digit. Refill before accepting it; rows are normally objects.
                if end == len(self.buffer) and not self.eof:
                    self.fill()
                    continue
                self.offset = end
                return value
            except json.JSONDecodeError as error:
                if self.eof:
                    raise ValueError('Invalid or truncated semantic wire JSON') from error
                if (error.pos < len(self.buffer) - 32 and
                        not error.msg.startswith('Unterminated string')):
                    raise ValueError('Invalid semantic wire JSON') from error
                if not self.fill():
                    raise ValueError('Invalid or truncated semantic wire JSON') from error

    def rows(self):
        self.take('[')
        if self.peek() == ']':
            self.take(']')
            return
        while True:
            yield self.value()
            if self.peek() == ']':
                self.take(']')
                return
            self.take(',')

    def entries(self):
        self.take('{')
        seen = set()
        if self.peek() == '}':
            self.take('}')
        else:
            while True:
                name = self.value()
                if not isinstance(name, str) or name in seen:
                    raise ValueError('Invalid or repeated semantic wire field')
                seen.add(name)
                self.take(':')
                yield name, self.rows() if name in ARRAYS else self.value()
                if self.peek() == '}':
                    self.take('}')
                    break
                self.take(',')
        if self.peek():
            raise ValueError('Trailing content after semantic wire')


def quote(value):
    return '"' + value.replace('"', '""') + '"'


def logical_hash(db):
    digest = hashlib.sha256(b'dotobj-semantic/1\n')

    def frame(value):
        digest.update(str(len(value)).encode('ascii') + b':' + value)

    for name, sql in db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' AND name<>'content_digest' ORDER BY name"):
        digest.update(b'T'); frame(name.encode('utf-8'))
        digest.update(b'S'); frame(sql.encode('utf-8'))
        keys = sorted((row[5], row[1]) for row in db.execute('PRAGMA table_info(' + quote(name) + ')') if row[5])
        if not keys:
            raise ValueError('No primary key: ' + name)
        for row in db.execute('SELECT * FROM ' + quote(name) + ' ORDER BY ' + ','.join(quote(k) for _, k in keys)):
            digest.update(b'R')
            for value in row:
                if value is None:
                    kind, data = b'5', b''
                elif isinstance(value, int):
                    kind, data = b'1', str(value).encode('ascii')
                elif isinstance(value, str):
                    kind, data = b'3', value.encode('utf-8')
                elif isinstance(value, bytes):
                    kind, data = b'4', value
                else:
                    raise ValueError('Unsupported SQLite cell')
                digest.update(kind); frame(data)
            digest.update(b'\n')
        digest.update(b'\n')
    return digest.hexdigest()


# Public graph-fields-v1 serialization primitives. Kept with the standalone packager.
_GRAPH_MAX_BYTES = 2147483647
_GRAPH_BLOCK_BYTES = 65536
_GRAPH_UINT64_MAX = (1 << 64) - 1

def _graph_varuint(value):
    if type(value) is not int or not 0 <= value <= _GRAPH_UINT64_MAX:
        raise ValueError('Graph integer outside unsigned 64-bit encoding')
    out = bytearray()
    while value >= 128:
        out.append((value & 127) | 128)
        value >>= 7
    out.append(value)
    return out


def _graph_canonical_hex(value):
    if not isinstance(value, str) or len(value) % 2 or re.fullmatch('[0-9a-f]*', value) is None:
        raise ValueError('Noncanonical graph bytes')
    return bytes.fromhex(value)


def _graph_encode_row(record, definition_id, definition):
    if type(record['nodeId']) is not int or record['nodeId'] <= 0:
        raise ValueError('Invalid graph field owner')
    if record['targetId'] is not None and (type(record['targetId']) is not int or record['targetId'] <= 0):
        raise ValueError('Invalid graph reference ID')
    if definition['valueKind'] != 'reference' and (record['targetId'] is not None or record['targetEntryKind'] is not None):
        raise ValueError('Primitive graph field carries a reference')
    out = _graph_varuint(record['nodeId']) + _graph_varuint(definition_id) + _graph_varuint(record['ordinal'])
    kind, value = definition['valueKind'], record['value']
    if kind == 'reference':
        if value is not None:
            raise ValueError('Reference carries a primitive payload')
        out += _graph_varuint(record['targetId'] or 0)
    elif kind in ('signed', 'unsigned'):
        if not isinstance(value, str):
            raise ValueError('Graph primitive is not canonical decimal text')
        number = int(value)
        if str(number) != value or (kind == 'signed' and not -(1 << 63) <= number < (1 << 63)):
            raise ValueError('Noncanonical graph primitive')
        out += _graph_varuint(((number << 1) ^ (number >> 63)) if kind == 'signed' else number)
    elif kind == 'boolean':
        if value not in ('0', '1'):
            raise ValueError('Invalid graph boolean')
        out.append(int(value))
    elif kind in ('text', 'bytes'):
        payload = value.encode('utf-8') if kind == 'text' else _graph_canonical_hex(value)
        if len(payload) > _GRAPH_MAX_BYTES:
            raise ValueError('Graph field exceeds supported byte length')
        out += _graph_varuint(len(payload)) + payload
    else:
        raise ValueError('Unknown graph primitive kind')
    return out


def _graph_stored_block(ordinal, row_count, data, encoding='graph-fields-v1'):
    if encoding not in ('graph-fields-v1', 'graph-groups-v1'):
        raise ValueError('Unknown graph field encoding')
    if type(ordinal) is not int or ordinal < 0 or type(row_count) is not int or row_count <= 0:
        raise ValueError('Invalid graph block counts')
    if not data or len(data) > _GRAPH_MAX_BYTES:
        raise ValueError('Unsupported graph block byte length')
    compressed = zlib.compress(data, level=6)
    encoded, codec = (compressed, 'zlib') if len(compressed) < len(data) else (data, 'raw')
    return ordinal, row_count, encoded, codec, len(data), hashlib.sha256(data).hexdigest(), encoding



def package(wire, path, source, unit_id, provenance, started_ns):
    """Compatibility entry point for already decoded fixture wires."""
    return package_entries(iter(wire.items()), path, source, unit_id, provenance, started_ns)


def package_file(wire_path, path, source, unit_id, provenance, started_ns):
    with Path(wire_path).open(encoding='utf-8') as stream:
        return package_entries(WireReader(stream).entries(), path, source, unit_id, provenance, started_ns)


def package_entries(entries, path, source, unit_id, provenance, started_ns):
    header, counts, seen = {}, {}, set()
    for kind, value in provenance:
        if kind == 'environment':
            captured_unit = json.loads(value).get('EDG_DOT_OBJECT_UNIT_ID')
            if captured_unit is not None and captured_unit != unit_id:
                raise ValueError('Owned translation unit identity differs from producer environment')
    source_hashes = {}
    with sqlite3.connect(path) as db:
        db.executescript(SCHEMA.read_text())
        # This is an unpublished staging database. Forward references and
        # cycles are expected; incremental deferred-FK bookkeeping scans child
        # tables whenever a missing parent arrives and becomes quadratic on
        # project-sized graphs. Validate all references once after the load.
        # CHECK/UNIQUE/NOT NULL constraints remain enforced on every insertion.
        db.execute('PRAGMA foreign_keys=OFF')
        db.execute('BEGIN IMMEDIATE')
        for index, (kind, value) in enumerate(provenance):
            db.execute('INSERT INTO provenance VALUES(?,?,?)', (index, kind, value))
        for name, records in entries:
            if name in seen:
                raise ValueError('Repeated semantic wire field: ' + name)
            seen.add(name)
            if name in ('formatVersion', 'profile', 'sourceLanguage'):
                header[name] = records
                continue
            if name == 'edgGraphNodes':
                if not isinstance(records, (list, types.GeneratorType)):
                    raise ValueError('Expected array: ' + name)
                counts[name] = 0
                fields, kinds = set(ARRAYS[name][1]), {}
                for record in records:
                    if not isinstance(record, dict) or set(record) != fields:
                        raise ValueError('Invalid fields in ' + name)
                    kind = record['entryKind']
                    metadata = (record['entryKindName'], record['capturePhase'])
                    if kind not in kinds:
                        db.execute('INSERT INTO edg_graph_node_kinds VALUES(?,?,?)',
                                   (kind, *metadata))
                        kinds[kind] = metadata
                    elif kinds[kind] != metadata:
                        raise ValueError('Inconsistent EDG node kind metadata')
                    db.execute('INSERT INTO edg_graph_node_entries VALUES(?,?)',
                               (record['id'], kind))
                    counts[name] += 1
                continue
            if name == 'edgGraphFields':
                # Diagnostic compatibility path: losslessly transcode expanded
                # typed rows into the same declared codec used by native output.
                # It is intentionally not used by the normal compact exporter.
                if 'edgGraphFieldDefinitions' in seen or 'edgGraphFieldBlocks' in seen:
                    raise ValueError('Mixed EDG graph field representations')
                definitions, payload, row_count, block_ordinal = {}, bytearray(), 0, 0
                fields = {'nodeId', 'name', 'ordinal', 'valueKind', 'value', 'targetId', 'targetEntryKind'}
                counts[name] = 0
                for record in records:
                    if not isinstance(record, dict) or set(record) != fields:
                        raise ValueError('Invalid expanded graph field')
                    node = db.execute('SELECT entry_kind FROM edg_graph_nodes WHERE id=?', (record['nodeId'],)).fetchone()
                    if node is None:
                        raise ValueError('Expanded graph field has no node')
                    key = (node[0], record['name'], record['valueKind'], record['targetEntryKind'])
                    if key not in definitions:
                        definition_id = len(definitions) + 1
                        definitions[key] = definition_id
                        db.execute('INSERT INTO edg_graph_field_definitions VALUES(?,?,?,?,?)', (definition_id, *key))
                    encoded = _graph_encode_row(record, definitions[key],
                        dict(valueKind=record['valueKind']))
                    if payload and len(payload) + len(encoded) > _GRAPH_BLOCK_BYTES:
                        db.execute('INSERT INTO edg_graph_field_blocks VALUES(?,?,?,?,?,?,?)',
                            _graph_stored_block(block_ordinal, row_count, payload))
                        block_ordinal += 1
                        payload, row_count = bytearray(), 0
                    payload += encoded
                    row_count += 1
                    counts[name] += 1
                if payload:
                    db.execute('INSERT INTO edg_graph_field_blocks VALUES(?,?,?,?,?,?,?)',
                        _graph_stored_block(block_ordinal, row_count, payload))
                continue
            if name not in ARRAYS:
                raise ValueError('Unexpected semantic wire field: ' + name)
            table, fields = ARRAYS[name]
            if not isinstance(records, (list, types.GeneratorType)):
                raise ValueError('Expected array: ' + name)
            counts[name] = 0
            columns = [re.sub(r'[A-Z]', lambda match: '_' + match[0].lower(), field) for field in fields]
            if name == 'storageData':
                columns[-1] = 'bytes'
            if name == 'edgGraphFieldBlocks':
                if 'edgGraphFields' in seen:
                    raise ValueError('Mixed EDG graph field representations')
                columns = ['ordinal', 'row_count', 'bytes', 'codec', 'decoded_byte_length', 'sha256', 'encoding']
            if name == 'edgGraphFieldDefinitions' and 'edgGraphFields' in seen:
                raise ValueError('Mixed EDG graph field representations')
            if name == 'sourceFiles':
                columns += ['available', 'sha256', 'bytes', 'codec', 'decoded_byte_length']
            schema_columns = [row[1] for row in db.execute('PRAGMA table_info(' + quote(table) + ')')]
            if len(columns) != len(set(columns)) or set(columns) != set(schema_columns):
                raise ValueError('Semantic field/schema mismatch: ' + name)
            insert = ('INSERT INTO ' + quote(table) + '(' + ','.join(map(quote, columns)) +
                      ') VALUES(' + ','.join('?' for _ in columns) + ')')
            for record in records:
                counts[name] += 1
                if not isinstance(record, dict) or set(record) != set(fields):
                    raise ValueError('Invalid fields in ' + name + ': ' + str(sorted(record) if isinstance(record, dict) else type(record).__name__))
                values = [record[key] for key in fields]
                if name == 'edgGraphFieldBlocks':
                    values = _graph_stored_block(record['blockOrdinal'], record['rowCount'],
                                                       _graph_canonical_hex(record['dataHex']), record['encoding'])
                if name == 'storageData':
                    data = record['dataHex']
                    if (not isinstance(data, str) or not data or len(data) % 2
                            or any(c not in '0123456789abcdef' for c in data)):
                        raise ValueError('Noncanonical storage data hex')
                    values[-1] = bytes.fromhex(data)
                if name == 'sourceFiles':
                    file = Path(record['path'])
                    if file.is_file():
                        before = file.stat()
                        data = file.read_bytes()
                        after = file.stat()
                        if (before.st_mtime_ns, before.st_ctime_ns, before.st_size) != (after.st_mtime_ns, after.st_ctime_ns, after.st_size):
                            raise ValueError('Source changed during snapshot: ' + str(file))
                        if max(after.st_mtime_ns, after.st_ctime_ns) > started_ns:
                            raise ValueError('Source changed during compilation: ' + str(file))
                        digest = hashlib.sha256(data).hexdigest()
                        source_hashes[str(file)] = digest
                        if len(data) > 2147483647:
                            raise ValueError('Source snapshot exceeds supported decoded length: ' + str(file))
                        compressed = zlib.compress(data, level=9)
                        encoded, codec = (compressed, 'zlib') if len(compressed) < len(data) else (data, 'raw')
                        values += [1, digest, encoded, codec, len(data)]
                    else:
                        values += [0, None, None, None, None]
                db.execute(insert, values)
        expected = {'formatVersion', 'profile', 'sourceLanguage', *ARRAYS}
        if 'edgGraphFields' in seen:
            expected -= {'edgGraphFieldDefinitions', 'edgGraphFieldBlocks'}
            expected.add('edgGraphFields')
        if seen != expected:
            raise ValueError('Missing semantic wire fields')
        if header['formatVersion'] != 1 or header['profile'] != 'scalar-control-v66':
            raise ValueError('Unsupported semantic wire version/profile')
        if header['sourceLanguage'] not in ('c', 'c++'):
            raise ValueError('Unsupported source language')
        db.execute('INSERT INTO object_info VALUES(1,1,?,?,?,?,?,?,0,0,0,1,1,?)',
                   (header['profile'], 'portable-lp64-binary80-v2', 'dotobj-scalar-v1',
                    unit_id, str(source), sha(source), header['sourceLanguage']))
        errors = list(db.execute('PRAGMA foreign_key_check'))
        if errors:
            raise ValueError('Invalid semantic references: ' + str(errors[:10]))
        digest = logical_hash(db)
        db.execute('INSERT INTO content_digest VALUES(1,?,?)', ('dotobj-semantic-sha256-v1', digest))
        db.commit()
        db.execute('PRAGMA foreign_keys=ON')
        if db.execute('PRAGMA integrity_check').fetchone() != ('ok',):
            raise ValueError('SQLite integrity failure')
    for file, digest in source_hashes.items():
        if sha(file) != digest:
            raise ValueError('Source changed after snapshot: ' + file)
    return dict(profile=header['profile'], counts=counts)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--work-dir', required=True, type=Path)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--std', choices=['c11', 'c++14', 'c++17', 'c++20'], default='c++17')
    parser.add_argument('--unit-id')
    parser.add_argument('-I', dest='frontend_options', action=FrontendOption)
    parser.add_argument('-D', dest='frontend_options', action=FrontendOption)
    parser.add_argument('-U', dest='frontend_options', action=FrontendOption)
    parser.add_argument('--preinclude', dest='frontend_options', action=FrontendOption)
    parser.add_argument('--exceptions', action=argparse.BooleanOptionalAction, default=False)
    parser.add_argument('--rtti', action=argparse.BooleanOptionalAction, default=False)
    args = parser.parse_args()
    source, output, work, compiler = (p.resolve() for p in (args.source, args.output, args.work_dir, args.compiler))
    if output.suffix != '.dotobj' or output.exists():
        parser.error('--output must be a new .dotobj file')
    if not source.is_file():
        parser.error('Source does not exist')
    work.mkdir(parents=True, exist_ok=False)
    output.parent.mkdir(parents=True, exist_ok=True)
    unit_id = args.unit_id or hashlib.sha256(str(source).encode('utf-8')).hexdigest()
    command = [str(compiler), '--' + args.std, '--gnu_version', '130300',
               '--clear_flag', 'use_predefined_macro_file']
    if args.std.startswith('c++'):
        command += ['--instantiate', 'used',
                    '--exceptions' if args.exceptions else '--no_exceptions',
                    '--rtti' if args.rtti else '--no_rtti']
    elif args.exceptions or args.rtti:
        parser.error('--exceptions and --rtti require C++')
    for option, value in args.frontend_options or []:
        if option in ('-I', '--preinclude'):
            command += [option, str(Path(value).resolve())]
        else:
            command += [option + value]
    command += ['--', str(source)]
    env = dict(os.environ, EDG_BASE=str(ROOT / 'upstream/bases/docker/dev-env/gcc'),
               EDG_DOT_OBJECT='1', EDG_DOT_OBJECT_UNIT_ID=unit_id, EDG_DOT_TARGET='portable-lp64-binary80-v2',
               EDG_DOT_NAMESPACE='@DotObject', EDG_DOT_CLASS='@Unit',
               EDG_DOT_EMIT='managedlib', EDG_DOT_INSTANCE='0', EDG_DOT_NEST='1',
               EDG_DOT_LIBC='none', EDG_DOT_RUNTIME='none', EDG_DOT_POSIX='0')
    before, compiler_hash = sha(source), sha(compiler)
    started_ns = time.time_ns()
    invocation = dict(compiler=str(compiler), compilerSha256=compiler_hash, arguments=command[1:],
                      source=str(source), sourceSha256=before, unitId=unit_id,
                      environment={key: value for key, value in env.items() if key.startswith('EDG_')})
    (work / 'invocation.json').write_text(json.dumps(invocation, indent=2) + '\n')
    with (work / 'wire.json').open('wb') as stdout, (work / 'stderr').open('wb') as stderr:
        result = subprocess.run(command, cwd=work, env=env, stdout=stdout, stderr=stderr)
    if result.returncode:
        raise RuntimeError('Frontend failed; preserved diagnostic: ' + str(work / 'stderr'))
    if sha(source) != before or sha(compiler) != compiler_hash:
        raise ValueError('Source/compiler changed during compilation')
    partial = work / 'object.partial.sqlite'
    provenance = [('producer', 'dot-gen-owned-semantic-v1'), ('compiler-sha256', compiler_hash),
                  ('arguments', json.dumps(command[1:])), ('packager-sha256', sha(__file__)),
                  ('schema-sha256', sha(SCHEMA)), ('environment', json.dumps(invocation['environment'], sort_keys=True))]
    packaged = package_file(work / 'wire.json', partial, source, unit_id, provenance, started_ns)
    # Stage on the destination filesystem, then publish atomically without
    # overwriting another build. Retain both staged copies as campaign evidence.
    pending = output.with_name(output.name + '.pending')
    with partial.open('rb') as incoming, pending.open('xb') as outgoing:
        import shutil
        shutil.copyfileobj(incoming, outgoing)
        outgoing.flush()
        os.fsync(outgoing.fileno())
    os.link(pending, output)
    report = dict(status='packaged-semantic-object', object=str(output), sha256=sha(output),
                  profile=packaged['profile'], frontendInvocations=1, executionVerified=False,
                  capabilities=dict(prelowerSource=False, macros=False, lifetimes=False, lexicalCleanups=True, sourceDeclarations=True, macroDefinitions=True),
                  counts=packaged['counts'])
    (work / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print(json.dumps(report))


if __name__ == '__main__':
    main()
