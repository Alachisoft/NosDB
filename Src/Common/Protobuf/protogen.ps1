$protoc_path =  "..\..\..\Resources\NugetPackages\Google.ProtocolBuffers.2.4.1.555\tools"

try{
"Usman's cool new proto generation automation script"

cd $protoc_path

foreach($proto in get-ChildItem *.proto -Path "..\..\..\..\Src\Common\Protobuf\DatabaseOperations"){
.\ProtoGen.exe $proto.Name --proto_path="..\..\..\..\Src\Common\Protobuf\DatabaseOperations" --include_imports -output_directory="..\..\..\..\Src\Common\Protobuf\DatabaseOperations"
"CS generated for " + $proto.FullName
}

foreach($proto in get-ChildItem *.proto -Path "..\..\..\..\Src\Common\Protobuf\DatabaseOperations\Commands"){
.\ProtoGen.exe $proto.Name --proto_path="E:\NCacheDB Clone\Src\Common\Protobuf\DatabaseOperations\Commands" --proto_path="E:\NCacheDB Clone\Src\Common\Protobuf\ConfigurationDomProtos" -output_directory="..\..\..\..\Src\Common\Protobuf\DatabaseOperations\Commands"
"CS generated for " + $proto.FullName
}

foreach($proto in get-ChildItem *.proto -Path "..\..\..\..\Src\Common\Protobuf\ConfigurationDomProtos"){
.\ProtoGen.exe $proto.Name --proto_path="..\..\..\..\Src\Common\Protobuf\ConfigurationDomProtos" --include_imports -output_directory="..\..\..\..\Src\Common\Protobuf\ConfigurationDomProtos"
"CS generated for " + $proto.FullName
}

foreach($proto in get-ChildItem *.proto -Path "..\..\..\..\Src\Common\Protobuf\ManagementCommands"){
.\ProtoGen.exe $proto.Name --proto_path="..\..\..\..\Src\Common\Protobuf\ManagementCommands" --include_imports -output_directory="..\..\..\..\Src\Common\Protobuf\ManagementCommands"
"CS generated for " + $proto.FullName
}

foreach($proto in get-ChildItem *.proto -Path "..\..\..\..\Src\Common\Protobuf\ManagementCommands\Commands"){
.\ProtoGen.exe $proto.Name --proto_path="..\..\..\..\Src\Common\Protobuf\ManagementCommands" --include_imports -output_directory="..\..\..\..\Src\Common\Protobuf\ManagementCommands\Commands"
"CS generated for " + $proto.FullName
}

"All Done!"
Read-Host

}
catch{
echo $_.Exception|format-list -force

Read-Host
}