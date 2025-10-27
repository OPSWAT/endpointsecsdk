CXX = g++
CXXFLAGS = -std=c++17 -Iinclude -I../sdk/inc -L../sdk/lib/x64 -Wl,-rpath=../sdk/lib/x64
LDFLAGS = -lnlohmann_json
LIBS = -lwaapi

SRC = GetMissingPatches.cpp Util.cpp SDKInit.cpp
OUT = get-missing-patches

all:
	$(CXX) $(CXXFLAGS) $(SRC) -o $(OUT) $(LIBS)

clean:
	rm -f $(OUT)