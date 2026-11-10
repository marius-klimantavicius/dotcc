cmake_minimum_required(VERSION 3.14)
project(cui_gallery_control C)
if(NOT CUI_SOURCE_ROOT)
  message(FATAL_ERROR "CUI_SOURCE_ROOT must name the pinned upstream source")
endif()
set(CUI_PLATFORM glfw CACHE STRING "" FORCE)
set(CUI_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(CUI_BUILD_TESTS OFF CACHE BOOL "" FORCE)
set(CUI_TEXT_SHAPING ON CACHE BOOL "" FORCE)
set(CUI_IMAGE_ALL_FORMATS ON CACHE BOOL "" FORCE)
add_subdirectory("${CUI_SOURCE_ROOT}" upstream)
add_executable(gallery-control native.c native-input.c
  "${CUI_SOURCE_ROOT}/examples/gallery_ui.c"
  "${CUI_SOURCE_ROOT}/examples/gallery_more.c")
target_include_directories(gallery-control PRIVATE
  "${CUI_SOURCE_ROOT}/examples" "${CUI_SOURCE_ROOT}/src")
target_link_libraries(gallery-control PRIVATE cui::cui)
