#General vars
ARGS?=/restore /p:Configuration=Release
VS_PATH?=/Applications/Visual\ Studio\ \(Preview\).app
VS_DEBUG_PATH?=../vsmac/main/build/bin/VisualStudio.app

all:
	echo "Building MonoDevelop.FeatureSwitch..."
	msbuild /restore src/MonoDevelop.FeatureSwitch.sln

clean:
	find . -type d -name bin -exec rm -rf {} \;
	find . -type d -name obj -exec rm -rf {} \;
	find . -type d -name packages -exec rm -rf {} \;

pack:
	mono $(VS_PATH)/Contents/MonoBundle/vstool.exe setup pack bin/MonoDevelop.FeatureSwitch.dll

pack_debug:
	mono $(VS_DEBUG_PATH)/Contents/MonoBundle/vstool.exe setup pack bin/MonoDevelop.FeatureSwitch.dll

install: pack
	mono $(VS_PATH)/Contents/MonoBundle/vstool.exe setup install ./MonoDevelop.FeatureSwitch_0.1.mpack

install_debug: pack_debug
	mono $(VS_DEBUG_PATH)/Contents/MonoBundle/vstool.exe setup install ./MonoDevelop.FeatureSwitch_0.1.mpack

.PHONY: all clean pack install submodules sdk nuget-download check-dependencies
