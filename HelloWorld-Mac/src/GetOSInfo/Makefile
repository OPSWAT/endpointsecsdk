CXX = clang++
CXXFLAGS = -std=c++17 -Iinclude -I../sdk/inc
# Detect architecture and set appropriate Homebrew paths
ifeq ($(shell uname -m),arm64)
	CXXFLAGS += -I/opt/homebrew/include
	ARCH = arm64
else
	CXXFLAGS += -I/usr/local/include
	ARCH = x64
endif
LDFLAGS = -lnlohmann_json
LIBS = -L../sdk/lib/$(ARCH) -lwaapi
REQUIRED_LIBS = libwaapi.dylib libwaaddon.dylib libwacollector.dylib libwadeviceinfo.dylib libwadriver.dylib libwainfection.dylib libwalocal.dylib libwaremoval.dylib libwaresource.dylib libwautils.dylib libwavmodapi.dylib

SRC = GetOSInfo.cpp Util.cpp SDKInit.cpp
OUT = get-os-info

all: frameworks
	$(CXX) $(CXXFLAGS) $(SRC) -o $(OUT) $(LIBS) -Wl,-rpath,@loader_path/../Frameworks
	@echo "Build complete. Run with: ./$(OUT)"

frameworks:
	@echo "Creating Frameworks directory structure..."
	@mkdir -p ../Frameworks
	@for lib in $(REQUIRED_LIBS); do \
		cp -f ../sdk/lib/$(ARCH)/$$lib ../Frameworks/; \
	done

clean:
	rm -f $(OUT) 
	@echo "Removing Frameworks directory..."
	@rm -rf ../Frameworks 