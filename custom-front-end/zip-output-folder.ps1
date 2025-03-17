$sourceDir = $args[0] + "*"
$outputFile = $args[0] + "output.zip"

write-host $sourceDir
write-host $outputFile 

if (Test-Path $outputFile) {
    Remove-Item  $outputFile 
}

Compress-Archive -Path $sourceDir  -DestinationPath $outputFile