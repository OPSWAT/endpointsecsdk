CXX = g++
CXXFLAGS = -std=c++17 -Iinclude -I../sdk/inc -L../sdk/lib/x64 -Wl,-rpath=../sdk/lib/x64
LDFLAGS = -lnlohmann_json
LIBS = -lwaapi

SRC = DetectProducts.cpp Util.cpp SDKInit.cpp
OUT = detect_products

all:
	$(CXX) $(CXXFLAGS) $(SRC) -o $(OUT) $(LIBS)

clean:
	rm -f $(OUT)