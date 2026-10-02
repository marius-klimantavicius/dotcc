# Original C backend with EDG's supported canonical-order EH configuration.
macro_configuration_to_compile_definitions("cpfe-dot-oracle")
set_source_files_properties(${CORE_FRONT_END_SOURCE_FILES} PROPERTIES LANGUAGE CXX)
add_executable(cpfe-dot-oracle ${CORE_FRONT_END_SOURCE_FILES} ${TXT_FILES})
add_dependencies(cpfe-dot-oracle error-info)
add_softfloat_to_target(cpfe-dot-oracle)
