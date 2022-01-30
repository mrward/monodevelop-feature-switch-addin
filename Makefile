#General vars
VS_PATH?=/Applications/Visual\ Studio\ \(Preview\).app
VS_DEBUG_PATH?=../vsmac/main/build/bin/VisualStudio.app

PROJECT_NAME=MonoDevelop.FeatureSwitch
PROJECT_VERSION=0.4

all:
	echo "Building $(PROJECT_NAME)..."
	msbuild src/$(PROJECT_NAME).sln

clean:
	find . -type d -name bin -exec rm -rf {} \;
	find . -type d -name obj -exec rm -rf {} \;

pack:
	$(VS_PATH)/Contents/MacOS/vstool setup pack "$(CURDIR)/bin/$(PROJECT_NAME).dll" -d:"$(CURDIR)"

pack_debug:
	$(VS_DEBUG_PATH)/Contents/MacOS/vstool setup pack "$(CURDIR)/bin/$(PROJECT_NAME).dll" -d:"$(CURDIR)"

install: pack
	$(VS_PATH)/Contents/MacOS/vstool setup install "$(CURDIR)/$(PROJECT_NAME)_$(PROJECT_VERSION).mpack"

install_debug: pack_debug
	$(VS_DEBUG_PATH)/Contents/MacOS/vstool setup install "$(CURDIR)/$(PROJECT_NAME)_$(PROJECT_VERSION).mpack"

.PHONY: all clean pack pack_debug install install_debug
