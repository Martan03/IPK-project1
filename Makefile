.PHONY: build, submit, clean

build:
	dotnet publish -p:PublishSingleFile=true
	mv bin/Release/*/*/publish/IPK-project1 ipk24chat-client

submit:
	zip -r xsleza26.zip TCP UDP *.cs CHANGELOG.md class_diagram.png \
		IPK-project1.csproj LICENSE Makefile README.md tests/*.cs \
		tests/IPK-project1.Tests.csproj
