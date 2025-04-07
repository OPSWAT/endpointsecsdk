# Compiler and flags
CXX = clang++
CXXFLAGS = -std=c++17 -Wall -I/opt/homebrew/include
LDFLAGS = -L/opt/homebrew/lib -lcurl -ltinyxml2

# Target executable
TARGET = SDKDownloader

# Source files
SRC = SDKDownloader.cpp SDKFiles.cpp Util.cpp CatalogFiles.cpp

# Default target
all: $(TARGET)

# Force recompile every time
$(TARGET): $(SRC)
	$(CXX) $(CXXFLAGS) $(SRC) -o $(TARGET) $(LDFLAGS)
	touch $@

# Clean build files
clean:
	rm -f $(TARGET)

# Run the program
run: $(TARGET)
	./$(TARGET)

.PHONY: all clean run 