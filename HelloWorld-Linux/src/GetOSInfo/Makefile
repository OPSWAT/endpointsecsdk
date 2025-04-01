CXX = g++
CXXFLAGS = -std=c++17 -Iinclude -I../sdk/inc -L../sdk/lib/x64 -Wl,-rpath=../sdk/lib/x64
LDFLAGS = -lnlohmann_json
LIBS = -lwaapi

SRC = GetOSInfo.cpp Util.cpp SDKInit.cpp
OUT = get-os-info

all:
	$(CXX) $(CXXFLAGS) $(SRC) -o $(OUT) $(LIBS)

clean:
	rm -f $(OUT)