# Add the authored reporting adapter without modifying pinned DuckDB sources.
# Defer until upstream has defined unittest; nested project() calls use the guard.
if(NOT DEFINED DOTCC_DUCKDB_REPORTER_HOOK_ADDED)
  set(DOTCC_DUCKDB_REPORTER_HOOK_ADDED TRUE)
  function(dotcc_duckdb_add_reporter)
    if(TARGET unittest)
      target_sources(unittest PRIVATE "${CMAKE_CURRENT_FUNCTION_LIST_DIR}/tests/reporter.cpp")
    endif()
  endfunction()
  cmake_language(DEFER DIRECTORY "${CMAKE_SOURCE_DIR}" CALL dotcc_duckdb_add_reporter)
endif()
