CXX = clang++
CXXFLAGS = -std=c++17 -Wall -I/opt/homebrew/include
LDFLAGS = -L/opt/homebrew/lib -lcurl -ltinyxml2

# For ARM64 architecture (M1/M2 Macs)
ifeq ($(shell uname -m),arm64)
	CXXFLAGS += -arch arm64
	LDFLAGS += -arch arm64
else
	CXXFLAGS += -arch x86_64
	LDFLAGS += -arch x86_64
endif

SRCS = main.cpp download_manager.cpp xml_parser.cpp package_extractor.cpp sdk_downloader.cpp
OBJS = $(SRCS:.cpp=.o)
TARGET = SDKDownloader

.PHONY: all clean

all: $(TARGET)

$(TARGET): $(OBJS)
	$(CXX) $(CXXFLAGS) -o $@ $^ $(LDFLAGS)

%.o: %.cpp
	$(CXX) $(CXXFLAGS) -c $< -o $@

clean:
	rm -f $(OBJS) $(TARGET) 