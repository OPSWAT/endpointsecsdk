CXX = clang++
CXXFLAGS = -std=c++17 -Iinclude -I../sdk/inc
# Detect architecture and set appropriate Homebrew paths
ifeq ($(shell uname -m),arm64)
	CXXFLAGS += -I/opt/homebrew/include
else
	CXXFLAGS += -I/usr/local/include
endif
LDFLAGS = -lnlohmann_json
# Force using x64 libraries for now since arm64 directory is empty
ARCH = x64
LIBS = -L../sdk/lib/$(ARCH) -lwaapi
REQUIRED_LIBS = libwaapi.dylib

SRC = GetMissingPatches.cpp Util.cpp SDKInit.cpp
OUT = get-missing-patches

all: symlinks
	$(CXX) $(CXXFLAGS) $(SRC) -o $(OUT) $(LIBS) -Wl,-rpath,../sdk/lib/$(ARCH)
	@echo "Build complete. Run with: ./$(OUT)"

symlinks:
	@echo "Creating symbolic links to required libraries..."
	@for lib in $(REQUIRED_LIBS); do \
		ln -sf ../sdk/lib/$(ARCH)/$$lib .; \
	done

clean:
	rm -f $(OUT) 
	@echo "Removing symbolic links..."
	@for lib in $(REQUIRED_LIBS); do \
		rm -f $$lib; \
	done 