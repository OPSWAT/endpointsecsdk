cmake_minimum_required(VERSION 3.15)
project(sdk_downloader VERSION 1.0.0 LANGUAGES CXX)

# Set C++17 standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Find required packages
find_package(CURL REQUIRED)
find_package(tinyxml2 REQUIRED)

# Set source files
set(SOURCES
    main.cpp
    download_manager.cpp
    xml_parser.cpp
    package_extractor.cpp
    sdk_downloader.cpp
)

# Create executable
add_executable(sdk_downloader ${SOURCES})

# Include directories
target_include_directories(sdk_downloader PRIVATE 
    ${CMAKE_CURRENT_SOURCE_DIR}
    ${CURL_INCLUDE_DIRS}
    ${TINYXML2_INCLUDE_DIRS}
)

# Link libraries
target_link_libraries(sdk_downloader PRIVATE 
    ${CURL_LIBRARIES}
    tinyxml2::tinyxml2
) 